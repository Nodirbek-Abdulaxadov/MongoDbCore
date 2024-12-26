namespace MongoDbCore.Identity.Models;

public class UserToken : BaseEntity
{
    public string UserId { get; set; } = NewId;
    public string Value { get; set; } = string.Empty;
    public DateTimeOffset ExpireAt { get; set; }
}