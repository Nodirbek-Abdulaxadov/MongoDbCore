namespace MongoDbCore;

/// <summary>Excludes the annotated property from BSON serialization.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class IgnoreThis : BsonIgnoreAttribute
{
}
