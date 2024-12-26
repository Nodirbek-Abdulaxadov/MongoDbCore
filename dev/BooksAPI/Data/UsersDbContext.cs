using MongoDbCore.Identity.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Data;

public class UsersDbContext(MongoDbCoreOptions options) : IdentityDbContext<User>(options)
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
}