namespace MongoDbCore;

/// <summary>
/// Records changes against entities decorated with <see cref="Auditable"/>.
/// </summary>
public interface IAuditService
{
    /// <summary>User identifier attached to each audit entry.</summary>
    string? User { get; set; }

    /// <summary>Records a single audit event.</summary>
    void Add(AuditEntity entity);

    /// <summary>Paginates audit events.</summary>
    Task<(List<AuditEntity> Items, long Count)> GetAllAsync(int pageSize = 10, int page = 1, CancellationToken cancellationToken = default);

    /// <summary>Looks up a single audit event by id.</summary>
    Task<AuditEntity?> GetAsync(string id, CancellationToken cancellationToken = default);
}
