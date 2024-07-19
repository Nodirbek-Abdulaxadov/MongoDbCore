namespace MongoDbCore;

[BsonIgnoreExtraElements(Inherited = true)]
public abstract class BaseEntity
{
    [Key]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(5);
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(5);
}