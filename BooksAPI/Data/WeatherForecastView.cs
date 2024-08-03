using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApplication1.Data;

[BsonIgnoreExtraElements]
public class WeatherForecastView
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public string? SummaryA { get; set; }
}
