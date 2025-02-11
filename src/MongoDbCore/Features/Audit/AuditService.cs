public class AuditService : IAuditService, IDisposable
{
    public dynamic? User { get; set; } = "me";

    public void Add(AuditEntity entity)
    {
        if (User is null)
        {
            throw new InvalidOperationException("Audit user not initialized!");
        }

        entity.User = User;
        StaticServiceLocator.DbContext!.Audits.Add(entity);
    }

    public async Task<(List<AuditEntity> Items, long count)> GetAll(int pageSize = 10, int page = 1, CancellationToken cancellationToken = default)
    {
        var count = await StaticServiceLocator.DbContext!.Audits.CountAsync();
        var items = await StaticServiceLocator.DbContext!.Audits.AsFindFluent().Skip(pageSize * (page - 1)).TakeAsync(pageSize, cancellationToken);

        return (items, count);
    }

    public Task<AuditEntity> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        return StaticServiceLocator.DbContext!.Audits.FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}