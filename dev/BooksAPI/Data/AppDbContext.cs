namespace WebApplication1.Data;

public class AppDbContext : MongoDbContext
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public SelfCachedCollection<WeatherForecast2> WeatherForecastsCached { get; set; } = null!;
}