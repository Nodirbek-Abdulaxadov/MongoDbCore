public class AppDbContext : MongoDbContext
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public SelfCachedCollection<WeatherForecast> WeatherForecastsCached { get; set; } = null!;
    public Collection<CityEntity> Cities { get; set; } = null!;
    public Collection<Country> Countries { get; set; } = null!;

    protected override void OnInitialized()
    {
        WeatherForecastsCached.ReloadCache();
        base.OnInitialized();
    }
}