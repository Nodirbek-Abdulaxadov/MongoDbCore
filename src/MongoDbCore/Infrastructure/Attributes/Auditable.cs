namespace MongoDbCore;

/// <summary>
/// Marks an entity for auditing. When applied, every mutation flowing through
/// <see cref="Collection{T}"/> is recorded via <see cref="IAuditService"/>.
/// Requires <see cref="MongoDbCoreOptions.EnableAuditing"/> to be <c>true</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class Auditable : Attribute
{
}
