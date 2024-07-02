namespace MongoDbCore.Caching;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class Cacheable : Attribute
{
}