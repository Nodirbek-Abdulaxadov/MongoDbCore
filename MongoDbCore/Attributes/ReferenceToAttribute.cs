namespace MongoDbCore.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ReferenceTo : Attribute
{
    public string? Entity { get; set; } = string.Empty;
}