using CareNotes.Application.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CareNotes.Infrastructure.Persistence;

internal static class MongoErrors
{
    public static bool IsObjectId(string? id) => ObjectId.TryParse(id, out _);

    /// Runs a write and converts duplicate-key errors into a 409 conflict.
    public static async Task<T> GuardDuplicateAsync<T>(Func<Task<T>> write, string message)
    {
        try
        {
            return await write();
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ConflictException(message);
        }
    }
}
