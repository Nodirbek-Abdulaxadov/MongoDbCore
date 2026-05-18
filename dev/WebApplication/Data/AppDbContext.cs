public class AppDbContext(MongoDbCoreOptions options) : MongoDbContext(options)
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public SelfCachedCollection<WeatherForecast> WeatherForecastsCached { get; set; } = null!;
    public Collection<CityEntity> Cities { get; set; } = null!;
    public Collection<Country> Countries { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        WeatherForecastsCached.ReloadCache();
        return Task.CompletedTask;
    }
}