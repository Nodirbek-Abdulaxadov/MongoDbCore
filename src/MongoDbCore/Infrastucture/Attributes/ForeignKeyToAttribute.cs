[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ForeignKeyTo : Attribute
{
    public string? Entity { get; set; } = string.Empty;

    public ForeignKeyTo()
    {
    }

    public ForeignKeyTo(string Entity)
    {
        this.Entity = Entity;
    }
}