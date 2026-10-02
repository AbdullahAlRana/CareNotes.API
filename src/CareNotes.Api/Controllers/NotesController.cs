using CareNotes.Api.Infrastructure;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Application.Services;
using CareNotes.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notes")]
public class NotesController(NoteService notes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteDto>>> Mine([FromQuery] PageQuery paging, CancellationToken ct) =>
        await notes.ListAsync(User.ToCurrentUser().Id, paging, ct);

    [HttpGet("all")]
    [Authorize(Policy = Roles.Admin)]
    public async Task<ActionResult<PagedResult<NoteDto>>> All([FromQuery] string? ownerId, [FromQuery] PageQuery paging, CancellationToken ct) =>
        await notes.ListAsync(string.IsNullOrWhiteSpace(ownerId) ? null : ownerId, paging, ct);

    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> Get(string id, CancellationToken ct) =>
        await notes.GetAsync(User.ToCurrentUser(), id, ct);

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create(NoteRequest request, CancellationToken ct)
    {
        var note = await notes.CreateAsync(User.ToCurrentUser(), request, ct);
        return CreatedAtAction(nameof(Get), new { id = note.Id }, note);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<NoteDto>> Update(string id, NoteRequest request, CancellationToken ct) =>
        await notes.UpdateAsync(User.ToCurrentUser(), id, request, ct);

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await notes.DeleteAsync(User.ToCurrentUser(), id, ct);
        return NoContent();
    }
}
