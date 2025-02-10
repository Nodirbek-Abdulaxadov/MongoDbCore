[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ReferenceTo(string? Entity = "") : BsonRepresentationAttribute(BsonType.ObjectId)
{
    public string? Entity { get; set; } = Entity;
}