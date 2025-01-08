using MongoDbCore.Booster;
using System.Diagnostics;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController2 : ControllerBase
{
    private readonly Booster<AppDbContext, WeatherForecast> _booster;
    private readonly AppDbContext dbContext;
    public WeatherForecastController2(AppDbContext dbContext)
    {
        _booster = new();
        this.dbContext = dbContext;
        _booster.Initialize(dbContext);
    }

    [HttpGet]
    public IActionResult Get()
    {
        var list = _booster.GetAll();
        return Ok(list);
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        int threadCount = 1000;
        int operationsPerThread = 1; // Total operations = threadCount * operationsPerThread

        async Task<string> RunBoosterOperationsAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, threadCount).Select(async _ =>
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var model = _booster.Add(WeatherForecast.Random());
                    var all = _booster.GetAll();
                    model.Summary = Guid.NewGuid().ToString();
                    model.TemperatureC = 23;
                    model = _booster.Update(model);
                    model = _booster.Get(model.Id);
                    all = _booster.GetAll();
                }
            });

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            return $"""
        Booster Operations:
        -------------------
        Elapsed time (ms): {stopwatch.Elapsed.TotalMilliseconds}
        Elapsed time (s): {stopwatch.Elapsed.TotalSeconds}
        -------------------
        """;
        }

        async Task<string> RunDbContextOperationsAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, threadCount).Select(async _ =>
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var model = dbContext.WeatherForecasts.Add(WeatherForecast.Random());
                    var all = dbContext.WeatherForecasts.ToList();
                    model.Summary = Guid.NewGuid().ToString();
                    model.TemperatureC = 23;
                    model = dbContext.WeatherForecasts.Update(model);
                    model = dbContext.WeatherForecasts.FirstOrDefault(x => x.Id == model.Id);
                    all = dbContext.WeatherForecasts.ToList();
                }
            });

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            return $"""
        DbContext Operations:
        ---------------------
        Elapsed time (ms): {stopwatch.Elapsed.TotalMilliseconds}
        Elapsed time (s): {stopwatch.Elapsed.TotalSeconds}
        ---------------------
        """;
        }

        // Run both operations concurrently
        var boosterTask = RunBoosterOperationsAsync();
        var dbContextTask = RunDbContextOperationsAsync();

        await Task.WhenAll(boosterTask, dbContextTask);

        // Combine results
        var result = $"""
    Performance Test Results:
    =========================
    {boosterTask.Result}

    {dbContextTask.Result}
    """;

        return Ok(result);
    }

}