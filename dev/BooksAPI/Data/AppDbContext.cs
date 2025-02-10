using WebApplication1.Data.Models;

namespace WebApplication1.Data;

public class AppDbContext : MongoDbContext
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public Collection<WeatherForecast2> WeatherForecastsCached { get; set; } = null!;
    public Collection<CityEntity> Cities { get; set; } = null!;
    public Collection<Country> Countries { get; set; } = null!;

    protected override void OnInitialized()
    {
        /*WeatherForecastsCached.ReloadCache();
        Countries.ReloadCache();
        Cities.ReloadCache();*/

        base.OnInitialized();
    }
}