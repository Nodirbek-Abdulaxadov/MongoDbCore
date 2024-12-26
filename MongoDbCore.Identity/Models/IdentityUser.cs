namespace MongoDbCore.Identity.Models;

public class IdentityUser : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public bool PhoneNumberConfirmed { get; set; }
    public string PasswordHash { get; set; } = string.Empty;

    [IgnoreThis] public List<UserClaim> Claims { get; set; } = [];
    [IgnoreThis] public List<UserLogin> Logins { get; set; } = [];
    [IgnoreThis] public List<UserToken> Tokens { get; set; } = [];
    [IgnoreThis] public List<UserSession> Sessions { get; set; } = [];
}