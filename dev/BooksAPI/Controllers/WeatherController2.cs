using System.Diagnostics;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController2 : ControllerBase
{
    private readonly AppDbContext dbContext;
    public WeatherForecastController2(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var list = dbContext.WeatherForecastsCached.ToList();
        return Ok(list);
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        int threadCount = 100;
        int operationsPerThread = 10; // Total operations = threadCount * operationsPerThread

        async Task<string> RunBoosterOperationsAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, threadCount).Select(async _ =>
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    var model = dbContext.WeatherForecastsCached.Add(WeatherForecast2.Random());
                    var all = dbContext.WeatherForecastsCached.ToList();
                    model.Summary = Guid.NewGuid().ToString();
                    model.TemperatureC = 23;
                    model = dbContext.WeatherForecastsCached.Update(model);
                    model = dbContext.WeatherForecastsCached.FirstOrDefault(x => x.Id == model.Id);
                    all = dbContext.WeatherForecastsCached.ToList();
                }
            });

            await Task.WhenAll(tasks);
            stopwatch.Stop();

            return $"""
        Cached Operations:
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


        /*
         * 
         * after test 11250 records on db results:
         
        Performance Test Results:
        =========================
        Cached Operations:
        -------------------
        Elapsed time (ms): 21207.7009
        Elapsed time (s): 21.2077009
        -------------------

        DbContext Operations:
        ---------------------
        Elapsed time (ms): 150717.4013
        Elapsed time (s): 150.7174013
        ---------------------

         **/
    }

}