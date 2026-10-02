using CareNotes.Domain.Entities;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Schemas;

public sealed class UserSchema : MongoSchema<User>
{
    public UserSchema() : base("users")
    {
        Map();

        // Login, registration duplicate check and email uniqueness.
        Index(k => k.Ascending(u => u.Email), new CreateIndexOptions { Unique = true, Name = "email_unique" });

        // Multikey index backing the $match stage of the group-by-interests aggregation.
        Index(k => k.Ascending(u => u.Interests), new CreateIndexOptions { Name = "interests" });
    }
}
