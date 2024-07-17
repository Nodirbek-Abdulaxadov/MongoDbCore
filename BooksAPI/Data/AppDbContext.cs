namespace WebApplication1.Data;

public class AppDbContext : MongoDbContext
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public Collection<ClassA> ClassAs { get; set; } = null!;
    public Collection<ClassB> ClassBs { get; set; } = null!;
    public Collection<ClassC> ClassCs { get; set; } = null!;

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
                SummaryA = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            await WeatherForecasts.AddRangeAsync(mockData);
        }

        if (!ClassAs.Any())
            {
            var mockData = Enumerable.Range(1, 5).Select(index => new ClassA
            {
                Number = Random.Shared.Next(1, 100)
            })
            .ToArray();

            await ClassAs.AddRangeAsync(mockData);
        }

        if (!ClassBs.Any())
        {
            var mockData = Enumerable.Range(1, 5).Select(index => new ClassB
            {
                Number = Random.Shared.Next(1, 100),
                ClassAId = ClassAs[Random.Shared.Next(1, 5)].Id
            })
            .ToArray();

            await ClassBs.AddRangeAsync(mockData);
        }

        if (!ClassCs.Any())
        {
            var mockData = Enumerable.Range(1, 5).Select(index => new ClassC
            {
                Number = Random.Shared.Next(1, 100),
                ClassBId = ClassBs[Random.Shared.Next(1, 5)].Id
            })
            .ToArray();

            await ClassCs.AddRangeAsync(mockData);
        }
    }
}