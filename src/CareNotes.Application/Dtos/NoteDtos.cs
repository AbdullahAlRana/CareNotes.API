using System.ComponentModel.DataAnnotations;
using CareNotes.Domain.Entities;

namespace CareNotes.Application.Dtos;

public record NoteRequest([Required, StringLength(200)] string Title, [StringLength(20000)] string? Content);

public record NoteDto(string Id, string OwnerId, string Title, string Content, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static NoteDto From(Note n) => new(n.Id, n.OwnerId, n.Title, n.Content, n.CreatedAt, n.UpdatedAt);
}
