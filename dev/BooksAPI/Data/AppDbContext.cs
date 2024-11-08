namespace WebApplication1.Data;

public class AppDbContext : MongoDbContext
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (!WeatherForecasts.Any())
        {
            string[] Summaries =
            [
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            ];
            var mockData = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            await WeatherForecasts.AddRangeAsync(mockData);
        }
    }
}