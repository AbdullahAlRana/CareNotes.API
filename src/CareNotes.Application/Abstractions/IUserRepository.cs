using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, long Total)> ListAsync(int skip, int limit, CancellationToken ct = default);
    Task CreateAsync(User user, CancellationToken ct = default);
    Task<bool> UpdateAsync(User user, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<PagedResult<InterestGroupDto>> GroupByInterestsAsync(IReadOnlyCollection<string> interests, PageQuery page, CancellationToken ct = default);
}
