namespace MongoDbCore.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class IgnoreThis : BsonIgnoreAttribute
{
}