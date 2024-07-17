namespace WebApplication1.Data;

public class ClassC : BaseEntity
{
    public int Number { get; set; }
    public string Address { get; set; } = string.Empty;
    [ForeignKeyTo(Entity = "ClassB")]
    public string ClassBId { get; set; } = string.Empty;
}
