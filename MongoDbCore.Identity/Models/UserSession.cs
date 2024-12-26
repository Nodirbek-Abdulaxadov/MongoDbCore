namespace MongoDbCore.Identity.Models;

public class UserSession : BaseEntity
{
    public DateTimeOffset? LastAccessedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string Device { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string UserId { get; set; } = NewId;
    public string TokenId { get; set; } = NewId;
}