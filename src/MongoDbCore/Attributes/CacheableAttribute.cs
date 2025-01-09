namespace MongoDbCore.Attributes;

[Obsolete("Cacheable is obsolete. It's not working anymore")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class Cacheable : Attribute
{
}