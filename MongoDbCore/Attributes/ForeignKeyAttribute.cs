namespace MongoDbCore.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ForeignKey : Attribute
{
    public string? Entity { get; set; } = string.Empty;
}