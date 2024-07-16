using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Data;

public class ClassA : BaseEntity
{
    public int Number { get; set; }
    [BsonIgnore]
    public ClassB? ClassB { get; set; }
    [ReferenceTo(Entity = "ClassC")]
    public ClassC ClassC { get; set; } = new ClassC()
    {
        Id = "66911121ee67f1dae005bd76"
    };

    [BsonIgnore]
    public List<ClassC>? ClassCList { get; set; }
}