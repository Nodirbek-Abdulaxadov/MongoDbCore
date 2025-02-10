namespace MongoDbCore.Identity.Data;

public class IdentityDbContext<TUser, TRole> : MongoDbContext
        where TUser : IdentityUser
        where TRole : IdentityRole
{
    public IdentityDbContext(MongoDbCoreOptions options)
        : base(options) { }

    public Collection<TUser> Users { get; set; } = null!;
    public Collection<UserClaim> UserClaims { get; set; } = null!;
    public Collection<UserLogin> UserLogins { get; set; } = null!;
    public Collection<UserToken> UserTokens { get; set; } = null!;
    public Collection<UserSession> UserSessions { get; set; } = null!;
    public Collection<TRole> Roles { get; set; } = null!;
    public Collection<RoleClaim> RoleClaims { get; set; } = null!;
}

public class IdentityDbContext<TUser> : MongoDbContext
        where TUser : IdentityUser
{
    public IdentityDbContext(MongoDbCoreOptions options) 
        : base(options) { }

    public Collection<TUser> Users { get; set; } = null!;
    public Collection<UserClaim> UserClaims { get; set; } = null!;
    public Collection<UserLogin> UserLogins { get; set; } = null!;
    public Collection<UserToken> UserTokens { get; set; } = null!;
    public Collection<UserSession> UserSessions { get; set; } = null!;
    public Collection<IdentityRole> Roles { get; set; } = null!;
    public Collection<RoleClaim> RoleClaims { get; set; } = null!;
}

public class IdentityDbContext : MongoDbContext
{
    public IdentityDbContext(MongoDbCoreOptions options) 
        : base(options) { }

    public Collection<IdentityUser> Users { get; set; } = null!;
    public Collection<UserClaim> UserClaims { get; set; } = null!;
    public Collection<UserLogin> UserLogins { get; set; } = null!;
    public Collection<UserToken> UserTokens { get; set; } = null!;
    public Collection<UserSession> UserSessions { get; set; } = null!;
    public Collection<IdentityRole> Roles { get; set; } = null!;
    public Collection<RoleClaim> RoleClaims { get; set; } = null!;
}