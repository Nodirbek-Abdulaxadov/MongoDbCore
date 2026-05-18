namespace MongoDbCore;

/// <summary>
/// Default implementation of <see cref="IAuditService"/>. Routes audit records
/// to the <c>Audits</c> collection on the active <see cref="MongoDbContext"/>.
/// </summary>
public class AuditService : IAuditService
{
    /// <inheritdoc />
    public string? User { get; set; } = "system";

    /// <inheritdoc />
    public void Add(AuditEntity entity)
    {
        var ctx = StaticServiceLocator.DbContext
            ?? throw new InvalidOperationException("MongoDbContext has not been initialized.");
        if (ctx.Audits is null)
            throw new InvalidOperationException("Auditing is not enabled. Set MongoDbCoreOptions.EnableAuditing = true.");

        entity.User = User ?? string.Empty;
        ctx.Audits.Add(entity);
    }

    /// <inheritdoc />
    public async Task<(List<AuditEntity> Items, long Count)> GetAllAsync(int pageSize = 10, int page = 1, CancellationToken cancellationToken = default)
    {
        var audits = RequireAuditsCollection();
        var count = await audits.CountAsync();
        var items = await audits
            .AsFindFluent()
            .Skip(pageSize * (page - 1))
            .TakeAsync(pageSize, cancellationToken);
        return (items, count);
    }

    /// <inheritdoc />
    public Task<AuditEntity?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var audits = RequireAuditsCollection();
        return audits.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)!;
    }

    private static Collection<AuditEntity> RequireAuditsCollection()
    {
        var ctx = StaticServiceLocator.DbContext
            ?? throw new InvalidOperationException("MongoDbContext has not been initialized.");
        return ctx.Audits
            ?? throw new InvalidOperationException("Auditing is not enabled. Set MongoDbCoreOptions.EnableAuditing = true.");
    }
}
