using CareNotes.Application.Abstractions;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Services;

public class NoteService(INoteRepository notes)
{
    /// Lists notes for one owner, or every note when ownerId is null (admin).
    public async Task<PagedResult<NoteDto>> ListAsync(string? ownerId, PageQuery page, CancellationToken ct = default)
    {
        var (items, total) = await notes.ListAsync(ownerId, page.Skip, page.PageSize, ct);
        return new PagedResult<NoteDto>(items.Select(NoteDto.From).ToList(), page.Page, page.PageSize, total);
    }

    /// Returns a note visible to its owner or an admin.
    public async Task<NoteDto> GetAsync(CurrentUser user, string id, CancellationToken ct = default)
    {
        var note = await FindAsync(id, ct);
        if (note.OwnerId != user.Id && !user.IsAdmin)
            throw new ForbiddenException();
        return NoteDto.From(note);
    }

    /// Creates a note owned by the caller.
    public async Task<NoteDto> CreateAsync(CurrentUser user, NoteRequest request, CancellationToken ct = default)
    {
        var note = new Note { OwnerId = user.Id, Title = request.Title.Trim(), Content = request.Content ?? string.Empty };
        await notes.CreateAsync(note, ct);
        return NoteDto.From(note);
    }

    /// Updates a note owned by the caller.
    public async Task<NoteDto> UpdateAsync(CurrentUser user, string id, NoteRequest request, CancellationToken ct = default)
    {
        var note = await FindOwnedAsync(user, id, ct);
        note.Title = request.Title.Trim();
        note.Content = request.Content ?? string.Empty;
        note.UpdatedAt = DateTime.UtcNow;
        await notes.UpdateAsync(note, ct);
        return NoteDto.From(note);
    }

    /// Deletes a note owned by the caller.
    public async Task DeleteAsync(CurrentUser user, string id, CancellationToken ct = default)
    {
        await FindOwnedAsync(user, id, ct);
        await notes.DeleteAsync(id, ct);
    }

    private async Task<Note> FindOwnedAsync(CurrentUser user, string id, CancellationToken ct)
    {
        var note = await FindAsync(id, ct);
        return note.OwnerId == user.Id ? note : throw new ForbiddenException();
    }

    private async Task<Note> FindAsync(string id, CancellationToken ct) =>
        await notes.GetByIdAsync(id, ct) ?? throw new NotFoundException("Note not found.");
}
