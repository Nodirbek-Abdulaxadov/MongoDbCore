public interface IAuditService
{
    dynamic? User { get; set; }

    void Add(AuditEntity entity);

    Task<(List<AuditEntity> Items, long count)> GetAll(int pageSize = 10, int page = 1, CancellationToken cancellationToken = default);

    Task<AuditEntity> GetAsync(string id, CancellationToken cancellationToken = default);
}