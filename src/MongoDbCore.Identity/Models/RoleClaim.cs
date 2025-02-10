namespace MongoDbCore.Identity.Models;

public class RoleClaim : BaseEntity
{
    public string RoleId { get; set; } = NewId;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}