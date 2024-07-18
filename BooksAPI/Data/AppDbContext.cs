namespace WebApplication1.Data;

public class AppDbContext : MongoDbContext
{
    public Collection<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public Collection<ClassA> ClassAs { get; set; } = null!;
    public Collection<ClassB> ClassBs { get; set; } = null!;
    public Collection<ClassC> ClassCs { get; set; } = null!;
    public Collection<ClassD> ClassDs { get; set; } = null!;

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
            var mockData = Enumerable.Range(1, 500).Select(index => new ClassA
            {
                Number = Random.Shared.Next(1, 1000)
            })
            .ToArray();

            ClassAs.AddRange(mockData);
        }

        if (!ClassBs.Any())
        {
            var mockData = Enumerable.Range(1, 1000).Select(index => new ClassB
            {
                Number = Random.Shared.Next(1, 1000),
                ClassAId = ClassAs[Random.Shared.Next(1, 500)].Id
            })
            .ToArray();

            ClassBs.AddRange(mockData);
        }

        if (!ClassCs.Any())
        {
            var mockData = Enumerable.Range(1, 2000).Select(index => new ClassC
            {
                Number = Random.Shared.Next(1, 1000),
                ClassBId = ClassBs[Random.Shared.Next(1, 1000)].Id
            })
            .ToArray();

            ClassCs.AddRange(mockData);
        }

        if (!ClassDs.Any())
        {
            var mockData = Enumerable.Range(1, 2000).Select(index => new ClassD
            {
                Number = Random.Shared.Next(1, 1000),
                ClassCId = ClassCs[Random.Shared.Next(1, 20)].Id
            })
            .ToArray();

            ClassDs.AddRange(mockData);
        }
    }
}