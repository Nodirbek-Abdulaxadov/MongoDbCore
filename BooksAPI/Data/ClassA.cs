using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Data;

public class ClassA : BaseEntity
{
    public int Number { get; set; }
    [BsonIgnore]
    public ClassB? ClassB { get; set; }
    [BsonIgnore]
    public List<ClassB> ClassBList { get; set; } = [];
}