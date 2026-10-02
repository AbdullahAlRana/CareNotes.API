using CareNotes.Domain.Entities;

namespace CareNotes.Application.Abstractions;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<(IReadOnlyList<Note> Items, long Total)> ListAsync(string? ownerId, int skip, int limit, CancellationToken ct = default);
    Task CreateAsync(Note note, CancellationToken ct = default);
    Task<bool> UpdateAsync(Note note, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task DeleteByOwnerAsync(string ownerId, CancellationToken ct = default);
}
