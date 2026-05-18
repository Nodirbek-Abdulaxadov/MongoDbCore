namespace MongoDbCore;

/// <summary>
/// Base class for all entities persisted via <see cref="Collection{T}"/> or
/// <see cref="SelfCachedCollection{T}"/>. Provides a Mongo-friendly identifier
/// and standard audit timestamps.
/// </summary>
[BsonIgnoreExtraElements(Inherited = true)]
public abstract class BaseEntity
{
    /// <summary>Mongo ObjectId encoded as a 24-character hex string.</summary>
    [Key]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    /// <summary>UTC timestamp when this entity was first inserted.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp of the most recent update.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Generates a fresh Mongo ObjectId string.</summary>
    public static string NewId => ObjectId.GenerateNewId().ToString();

    /// <inheritdoc />
    public override bool Equals(object? o)
        => o is BaseEntity other && other.Id == Id;

    /// <inheritdoc />
    public override int GetHashCode()
        => string.IsNullOrEmpty(Id) ? 0 : Id.GetHashCode();

    /// <summary>Generates a fresh Mongo ObjectId string.</summary>
    public static string GenerateNewId() => ObjectId.GenerateNewId().ToString();
}
