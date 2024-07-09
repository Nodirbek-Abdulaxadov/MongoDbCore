namespace MongoDbCore;

[BsonIgnoreExtraElements(Inherited = true)]
public abstract class BaseEntity
{
    [Key]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("id")]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow.AddHours(5);
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow.AddHours(5);
}