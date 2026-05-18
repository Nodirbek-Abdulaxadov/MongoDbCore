namespace MongoDbCore;

/// <summary>
/// Tags a string property as a Mongo ObjectId reference to another entity.
/// Used by <c>IncludeRef</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ReferenceTo(string? entity = "") : BsonRepresentationAttribute(BsonType.ObjectId)
{
    public string? Entity { get; set; } = entity;
}
