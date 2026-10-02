using CareNotes.Domain.Entities;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Schemas;

public sealed class PostSchema : MongoSchema<Post>
{
    public PostSchema() : base("posts")
    {
        Map(cm => cm.GetMemberMap(p => p.AuthorId).SetSerializer(ObjectIdString));

        // $lookup foreignField (authorId) + newest-first sort inside the lookup pipeline, also used for cascade delete.
        Index(k => k.Ascending(p => p.AuthorId).Descending(p => p.Id), new CreateIndexOptions { Name = "authorId_id" });
    }
}
