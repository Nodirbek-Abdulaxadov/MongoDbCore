namespace MongoDbCore.Identity.Models;

public class IdentityRole : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? Description { get; set; }

    [IgnoreThis] public List<RoleClaim> Claims { get; set; } = [];
}