using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence.Schemas;

public abstract class MongoSchema<T>
{
    private readonly List<CreateIndexModel<T>> _indexes = [];

    static MongoSchema()
    {
        ConventionRegistry.Register(
            "CareNotes",
            new ConventionPack { new CamelCaseElementNameConvention(), new IgnoreExtraElementsConvention(true) },
            t => t.Namespace == "CareNotes.Domain.Entities");
    }

    protected MongoSchema(string collectionName) => CollectionName = collectionName;

    public string CollectionName { get; }

    public IReadOnlyList<CreateIndexModel<T>> Indexes => _indexes;

    /// Registers the BSON class map; string ids are stored as ObjectId.
    protected void Map(Action<BsonClassMap<T>>? configure = null) =>
        BsonClassMap.TryRegisterClassMap<T>(cm =>
        {
            cm.AutoMap();
            cm.IdMemberMap
                .SetSerializer(new StringSerializer(BsonType.ObjectId))
                .SetIdGenerator(StringObjectIdGenerator.Instance);
            configure?.Invoke(cm);
        });

    /// Declares an index on the collection, equivalent to Mongoose's schema.index().
    protected void Index(Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> keys, CreateIndexOptions? options = null) =>
        _indexes.Add(new CreateIndexModel<T>(keys(Builders<T>.IndexKeys), options));

    /// Creates all declared indexes on the collection.
    public Task ApplyAsync(IMongoCollection<T> collection, CancellationToken ct = default) =>
        _indexes.Count == 0 ? Task.CompletedTask : collection.Indexes.CreateManyAsync(_indexes, ct);

    protected static StringSerializer ObjectIdString { get; } = new(BsonType.ObjectId);
}
