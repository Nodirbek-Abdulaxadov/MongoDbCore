namespace MongoDbCore.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class Cacheable : Attribute
{
}