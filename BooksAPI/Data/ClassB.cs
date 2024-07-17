using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Data;

public class ClassB : BaseEntity
{
    public int Number { get; set; }
    [ForeignKeyTo(Entity = "ClassA")]
    public string ClassAId { get; set; } = string.Empty;
    [BsonIgnore]
    public ClassC? ClassC { get; set; }
    [BsonIgnore]
    public List<ClassC> ClassCList { get; set; } = new();
}