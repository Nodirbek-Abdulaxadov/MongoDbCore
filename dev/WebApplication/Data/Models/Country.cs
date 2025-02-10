[Auditable]
public class Country : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public static Country Random() => new () { Name = Guid.NewGuid().ToString() };
}