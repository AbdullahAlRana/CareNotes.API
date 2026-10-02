using CareNotes.Domain.Entities;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Schemas;

public sealed class NoteSchema : MongoSchema<Note>
{
    public NoteSchema() : base("notes")
    {
        Map(cm => cm.GetMemberMap(n => n.OwnerId).SetSerializer(ObjectIdString));

        // "My notes" list: equality on ownerId + newest-first sort, also used for count and cascade delete.
        Index(k => k.Ascending(n => n.OwnerId).Descending(n => n.Id), new CreateIndexOptions { Name = "ownerId_id" });
    }
}
