namespace MongoDbCore;

/// <summary>
/// Declares the foreign key relationship from the annotated property to the
/// specified entity type. Used by <c>Include</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ForeignKeyTo : Attribute
{
    public string? Entity { get; set; } = string.Empty;

    public ForeignKeyTo() { }

    public ForeignKeyTo(string entity) => Entity = entity;
}
