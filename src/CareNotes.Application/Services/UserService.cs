using CareNotes.Application.Abstractions;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Services;

public class UserService(IUserRepository users, INoteRepository notes, IPostRepository posts, IPasswordHasher hasher)
{
    /// Returns a single user profile.
    public async Task<UserDto> GetAsync(string id, CancellationToken ct = default) =>
        UserDto.From(await FindAsync(id, ct));

    /// Lists all users, newest first.
    public async Task<PagedResult<UserDto>> ListAsync(PageQuery page, CancellationToken ct = default)
    {
        var (items, total) = await users.ListAsync(page.Skip, page.PageSize, ct);
        return new PagedResult<UserDto>(items.Select(UserDto.From).ToList(), page.Page, page.PageSize, total);
    }

    /// Creates a user with the given role (admin only).
    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            Role = request.Role,
            Interests = Interests.Normalize(request.Interests)
        };
        await users.CreateAsync(user, ct);
        return UserDto.From(user);
    }

    /// Updates any user's details, role and optionally password (admin only).
    public async Task<UserDto> UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await FindAsync(id, ct);
        var email = request.Email.Trim().ToLowerInvariant();
        if (email != user.Email && await users.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Email is already registered.");

        user.Name = request.Name.Trim();
        user.Email = email;
        user.Role = request.Role;
        user.Interests = Interests.Normalize(request.Interests);
        if (!string.IsNullOrEmpty(request.Password))
            user.PasswordHash = hasher.Hash(request.Password);

        await users.UpdateAsync(user, ct);
        return UserDto.From(user);
    }

    /// Updates the caller's own name and interests.
    public async Task<UserDto> UpdateProfileAsync(string id, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await FindAsync(id, ct);
        user.Name = request.Name.Trim();
        user.Interests = Interests.Normalize(request.Interests);
        await users.UpdateAsync(user, ct);
        return UserDto.From(user);
    }

    /// Deletes a user together with their notes and posts (admin only).
    public async Task DeleteAsync(string id, string currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
            throw new ForbiddenException("You cannot delete your own account.");
        if (!await users.DeleteAsync(id, ct))
            throw new NotFoundException("User not found.");

        await notes.DeleteByOwnerAsync(id, ct);
        await posts.DeleteByAuthorAsync(id, ct);
    }

    /// Groups users by interest, optionally restricted to the given interests.
    public Task<PagedResult<InterestGroupDto>> GroupByInterestsAsync(IEnumerable<string>? interests, PageQuery page, CancellationToken ct = default) =>
        users.GroupByInterestsAsync(Interests.Normalize(interests), page, ct);

    private async Task<User> FindAsync(string id, CancellationToken ct) =>
        await users.GetByIdAsync(id, ct) ?? throw new NotFoundException("User not found.");
}
