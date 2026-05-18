namespace MongoDbCore;

/// <summary>
/// Represents a single audited mutation against a tracked collection.
/// </summary>
public class AuditEntity : BaseEntity
{
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string Collection { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
}
