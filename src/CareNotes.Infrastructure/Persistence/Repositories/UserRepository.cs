using CareNotes.Application.Abstractions;
using CareNotes.Application.Common;
using CareNotes.Application.Dtos;
using CareNotes.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Repositories;

public class UserRepository(MongoContext context) : IUserRepository
{
    private const string DuplicateEmail = "Email is already registered.";
    private readonly IMongoCollection<User> _users = context.Users;

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default) =>
        MongoErrors.IsObjectId(id) ? await _users.Find(u => u.Id == id).FirstOrDefaultAsync(ct) : null;

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyList<User> Items, long Total)> ListAsync(int skip, int limit, CancellationToken ct = default)
    {
        var items = await _users.Find(FilterDefinition<User>.Empty)
            .SortByDescending(u => u.Id)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(ct);
        var total = await _users.EstimatedDocumentCountAsync(cancellationToken: ct);
        return (items, total);
    }

    public Task CreateAsync(User user, CancellationToken ct = default) =>
        MongoErrors.GuardDuplicateAsync(async () =>
        {
            await _users.InsertOneAsync(user, cancellationToken: ct);
            return true;
        }, DuplicateEmail);

    public Task<bool> UpdateAsync(User user, CancellationToken ct = default) =>
        MongoErrors.GuardDuplicateAsync(async () =>
            (await _users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct)).MatchedCount > 0,
            DuplicateEmail);

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default) =>
        MongoErrors.IsObjectId(id) && (await _users.DeleteOneAsync(u => u.Id == id, ct)).DeletedCount > 0;

    /// Groups users by interest using a single aggregate call; $facet returns the page and the total group count together.
    public async Task<PagedResult<InterestGroupDto>> GroupByInterestsAsync(IReadOnlyCollection<string> interests, PageQuery page, CancellationToken ct = default)
    {
        var filtered = interests.Count > 0;
        var interestMatch = filtered
            ? new BsonDocument("interests", new BsonDocument("$in", new BsonArray(interests)))
            : new BsonDocument("interests", new BsonDocument("$type", "string"));

        var pipeline = new List<BsonDocument>
        {
            new("$match", interestMatch),
            new("$project", new BsonDocument { { "name", 1 }, { "email", 1 }, { "interests", 1 } }),
            new("$unwind", "$interests")
        };
        if (filtered)
            pipeline.Add(new BsonDocument("$match", interestMatch));

        pipeline.AddRange(
        [
            new("$group", new BsonDocument
            {
                { "_id", "$interests" },
                { "count", new BsonDocument("$sum", 1) },
                { "users", new BsonDocument("$push", new BsonDocument
                    {
                        { "id", new BsonDocument("$toString", "$_id") },
                        { "name", "$name" },
                        { "email", "$email" }
                    })
                }
            }),
            new("$sort", new BsonDocument { { "count", -1 }, { "_id", 1 } }),
            new("$facet", new BsonDocument
            {
                { "items", new BsonArray { new BsonDocument("$skip", page.Skip), new BsonDocument("$limit", page.PageSize) } },
                { "total", new BsonArray { new BsonDocument("$count", "value") } }
            })
        ]);

        var result = await _users.Aggregate<BsonDocument>(pipeline, cancellationToken: ct).FirstOrDefaultAsync(ct);

        var groups = result["items"].AsBsonArray.Select(g => new InterestGroupDto(
            g["_id"].AsString,
            g["count"].AsInt32,
            g["users"].AsBsonArray.Select(u => new InterestMemberDto(u["id"].AsString, u["name"].AsString, u["email"].AsString)).ToList()
        )).ToList();
        var total = result["total"].AsBsonArray.FirstOrDefault()?["value"].ToInt64() ?? 0;

        return new PagedResult<InterestGroupDto>(groups, page.Page, page.PageSize, total);
    }
}
