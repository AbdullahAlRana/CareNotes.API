using CareNotes.Domain.Entities;
using CareNotes.Infrastructure.Persistence.Schemas;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence;

public class MongoSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "carenotes";
}

public class MongoContext
{
    private readonly UserSchema _userSchema;
    private readonly NoteSchema _noteSchema;
    private readonly PostSchema _postSchema;

    public MongoContext(IOptions<MongoSettings> options, UserSchema userSchema, NoteSchema noteSchema, PostSchema postSchema)
    {
        _userSchema = userSchema;
        _noteSchema = noteSchema;
        _postSchema = postSchema;

        var database = new MongoClient(options.Value.ConnectionString).GetDatabase(options.Value.Database);
        Users = database.GetCollection<User>(userSchema.CollectionName);
        Notes = database.GetCollection<Note>(noteSchema.CollectionName);
        Posts = database.GetCollection<Post>(postSchema.CollectionName);
    }

    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Note> Notes { get; }
    public IMongoCollection<Post> Posts { get; }

    /// Creates every index declared in the schemas.
    public async Task EnsureIndexesAsync(CancellationToken ct = default)
    {
        await _userSchema.ApplyAsync(Users, ct);
        await _noteSchema.ApplyAsync(Notes, ct);
        await _postSchema.ApplyAsync(Posts, ct);
    }
}
