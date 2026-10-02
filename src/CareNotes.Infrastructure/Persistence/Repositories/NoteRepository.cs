using CareNotes.Application.Abstractions;
using CareNotes.Domain.Entities;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Repositories;

public class NoteRepository(MongoContext context) : INoteRepository
{
    private readonly IMongoCollection<Note> _notes = context.Notes;

    public async Task<Note?> GetByIdAsync(string id, CancellationToken ct = default) =>
        MongoErrors.IsObjectId(id) ? await _notes.Find(n => n.Id == id).FirstOrDefaultAsync(ct) : null;

    public async Task<(IReadOnlyList<Note> Items, long Total)> ListAsync(string? ownerId, int skip, int limit, CancellationToken ct = default)
    {
        if (ownerId is not null && !MongoErrors.IsObjectId(ownerId))
            return ([], 0);

        var filter = ownerId is null ? FilterDefinition<Note>.Empty : Builders<Note>.Filter.Eq(n => n.OwnerId, ownerId);
        var items = await _notes.Find(filter)
            .SortByDescending(n => n.Id)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(ct);
        var total = ownerId is null
            ? await _notes.EstimatedDocumentCountAsync(cancellationToken: ct)
            : await _notes.CountDocumentsAsync(filter, cancellationToken: ct);
        return (items, total);
    }

    public Task CreateAsync(Note note, CancellationToken ct = default) =>
        _notes.InsertOneAsync(note, cancellationToken: ct);

    public async Task<bool> UpdateAsync(Note note, CancellationToken ct = default) =>
        (await _notes.ReplaceOneAsync(n => n.Id == note.Id, note, cancellationToken: ct)).MatchedCount > 0;

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default) =>
        MongoErrors.IsObjectId(id) && (await _notes.DeleteOneAsync(n => n.Id == id, ct)).DeletedCount > 0;

    public Task DeleteByOwnerAsync(string ownerId, CancellationToken ct = default) =>
        _notes.DeleteManyAsync(n => n.OwnerId == ownerId, ct);
}
