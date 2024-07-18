namespace WebApplication1.Data;

public class ClassD : BaseEntity
{
    public int Number { get; set; }
    public string Address { get; set; } = string.Empty;
    [ForeignKeyTo(Entity = "ClassC")]
    public string ClassCId { get; set; } = string.Empty;
}