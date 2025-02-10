[BsonIgnoreExtraElements(Inherited = true)]
public abstract class BaseEntity
{
    [Key]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTime.Now;

    public static string NewId => ObjectId.GenerateNewId().ToString();

    public override bool Equals(object? o)
    {
        if (o is BaseEntity obj && obj is not null)
            return obj.Id == Id;

        return false;
    }

    public override int GetHashCode() => string.IsNullOrEmpty(Id) ? 0 : Id.GetHashCode();

    public static string GenerateNewId() => ObjectId.GenerateNewId().ToString();
}