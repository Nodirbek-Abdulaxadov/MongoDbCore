namespace MongoDbCore.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ReferenceTo(string? Entity = "") : Attribute
{
    public string? Entity { get; set; } = Entity;
}