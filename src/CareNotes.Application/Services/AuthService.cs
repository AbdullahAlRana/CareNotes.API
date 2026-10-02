using CareNotes.Application.Abstractions;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Services;

public class AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
{
    /// Registers a new account with the User role and signs it in.
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.GetByEmailAsync(email, ct) is not null)
            throw new ConflictException("Email is already registered.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            Role = Roles.User,
            Interests = Interests.Normalize(request.Interests)
        };
        await users.CreateAsync(user, ct);
        return Issue(user);
    }

    /// Validates credentials and issues a JWT.
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException();
        return Issue(user);
    }

    private AuthResponse Issue(User user)
    {
        var (token, expiresAt) = tokens.Create(user);
        return new AuthResponse(token, expiresAt, UserDto.From(user));
    }
}
