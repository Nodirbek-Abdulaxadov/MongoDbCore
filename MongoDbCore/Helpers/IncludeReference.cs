using System.Reflection;

namespace MongoDbCore.Helpers;

public class IncludeReference
{
    public string Id { get; set; } = string.Empty;
    public PropertyInfo Property { get; set; } = null!;
    public dynamic Value { get; set; } = null!;
}