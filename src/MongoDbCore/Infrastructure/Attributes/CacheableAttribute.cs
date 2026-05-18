namespace MongoDbCore;

/// <summary>Legacy caching marker. Replaced by <see cref="SelfCachedCollection{T}"/>.</summary>
[Obsolete("Cacheable is no longer used. Use SelfCachedCollection<T> in your DbContext instead.")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public sealed class Cacheable : Attribute
{
}
