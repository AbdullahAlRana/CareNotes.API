using CareNotes.Application.Abstractions;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Repositories;

public class PostRepository(MongoContext context) : IPostRepository
{
    private readonly IMongoCollection<Post> _posts = context.Posts;
    private readonly IMongoCollection<User> _users = context.Users;

    public async Task<Post?> GetByIdAsync(string id, CancellationToken ct = default) =>
        MongoErrors.IsObjectId(id) ? await _posts.Find(p => p.Id == id).FirstOrDefaultAsync(ct) : null;

    public Task CreateAsync(Post post, CancellationToken ct = default) =>
        _posts.InsertOneAsync(post, cancellationToken: ct);

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default) =>
        MongoErrors.IsObjectId(id) && (await _posts.DeleteOneAsync(p => p.Id == id, ct)).DeletedCount > 0;

    public Task DeleteByAuthorAsync(string authorId, CancellationToken ct = default) =>
        _posts.DeleteManyAsync(p => p.AuthorId == authorId, ct);

    /// Lists all posts newest first, resolving author names via $lookup on users._id.
    public async Task<PagedResult<PostDto>> FeedAsync(PageQuery page, CancellationToken ct = default)
    {
        var pipeline = new[]
        {
            new BsonDocument("$sort", new BsonDocument("_id", -1)),
            new BsonDocument("$skip", page.Skip),
            new BsonDocument("$limit", page.PageSize),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", _users.CollectionNamespace.CollectionName },
                { "localField", "authorId" },
                { "foreignField", "_id" },
                { "pipeline", new BsonArray { new BsonDocument("$project", new BsonDocument("name", 1)) } },
                { "as", "author" }
            })
        };

        var docs = await _posts.Aggregate<BsonDocument>(pipeline, cancellationToken: ct).ToListAsync(ct);
        var total = await _posts.EstimatedDocumentCountAsync(cancellationToken: ct);
        var items = docs.Select(d => ToPost(d, d["author"].AsBsonArray.FirstOrDefault()?["name"].AsString)).ToList();
        return new PagedResult<PostDto>(items, page.Page, page.PageSize, total);
    }

    /// Fetches a user and a page of their posts in one aggregation on users with a $lookup into posts.
    public async Task<UserPostsDto?> GetUserPostsAsync(string userId, PageQuery page, CancellationToken ct = default)
    {
        if (!MongoErrors.IsObjectId(userId))
            return null;

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument("_id", ObjectId.Parse(userId))),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", _posts.CollectionNamespace.CollectionName },
                { "localField", "_id" },
                { "foreignField", "authorId" },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$sort", new BsonDocument("_id", -1)),
                        new BsonDocument("$facet", new BsonDocument
                        {
                            { "items", new BsonArray { new BsonDocument("$skip", page.Skip), new BsonDocument("$limit", page.PageSize) } },
                            { "total", new BsonArray { new BsonDocument("$count", "value") } }
                        })
                    }
                },
                { "as", "posts" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "name", 1 },
                { "posts", new BsonDocument("$first", "$posts") }
            })
        };

        var doc = await _users.Aggregate<BsonDocument>(pipeline, cancellationToken: ct).FirstOrDefaultAsync(ct);
        if (doc is null)
            return null;

        var name = doc["name"].AsString;
        var posts = doc["posts"].AsBsonDocument;
        var items = posts["items"].AsBsonArray.Select(p => ToPost(p.AsBsonDocument, name)).ToList();
        var total = posts["total"].AsBsonArray.FirstOrDefault()?["value"].ToInt64() ?? 0;

        return new UserPostsDto(userId, name, new PagedResult<PostDto>(items, page.Page, page.PageSize, total));
    }

    private static PostDto ToPost(BsonDocument d, string? authorName) => new(
        d["_id"].AsObjectId.ToString(),
        d["authorId"].AsObjectId.ToString(),
        authorName,
        d["title"].AsString,
        d["content"].AsString,
        d["createdAt"].ToUniversalTime());
}
