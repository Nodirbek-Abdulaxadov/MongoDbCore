using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Data;

[BsonIgnoreExtraElements]
public class Type2
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? SummaryC { get; set; }

    public string? SummaryAAA { get; set; }
}