namespace MongoDbCore.Helpers;

public class IncludeReference
{
    public byte Order { get; set; }
    public Source? Source { get; set; }
    public Source? Destination { get; set; }
}

public class Source
{
    public PropertyInfo? PropertyInfo { get; set; }
    public string? CollectionName { get; set; }
}