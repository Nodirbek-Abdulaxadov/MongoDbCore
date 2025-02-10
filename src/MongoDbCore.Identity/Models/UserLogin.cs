namespace MongoDbCore.Identity.Models;

public class UserLogin : BaseEntity
{
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string UserId { get; set; } = NewId;
}