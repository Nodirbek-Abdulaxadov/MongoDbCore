
namespace MongoDbCore.Identity.Internal;

public class RoleManager<TRole>(IdentityDbContext<IdentityUser, TRole> dbContext) 
    : IRoleManager<TRole> where TRole : IdentityRole
{
    public Task CreateAsync(TRole role, CancellationToken cancellationToken)
        => dbContext.Roles.AddAsync(role, cancellationToken);

    public Task DeleteAsync(TRole role, CancellationToken cancellationToken)
        => dbContext.Roles.DeleteAsync(role);

    public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken) 
        => dbContext.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

    public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken) 
        => dbContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, cancellationToken);

    public Task UpdateAsync(TRole role, CancellationToken cancellationToken) 
        => dbContext.Roles.UpdateAsync(role, cancellationToken);

    public void Dispose()
        => GC.SuppressFinalize(this);

    public Task<List<TRole>> GetRolesAsync(CancellationToken cancellationToken = default)
        => dbContext.Roles.ToListAsync(cancellationToken);
}