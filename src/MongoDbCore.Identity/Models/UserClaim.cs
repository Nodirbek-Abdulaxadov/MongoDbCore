namespace MongoDbCore.Identity.Models;

public class UserClaim : BaseEntity
{
    public string UserId { get; set; } = NewId;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}