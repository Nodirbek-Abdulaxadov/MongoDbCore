using LazyData;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace WebApplication1.Controllers;

[ApiController]
[Route("test")]
public class WeatherForecastController3 : ControllerBase
{
    private readonly AppDbContext dbContext;
    public WeatherForecastController3(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var data = dbContext.WeatherForecastsCached.ToList();
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed);
        WriteData();
        stopwatch = Stopwatch.StartNew();
        data = dbContext.WeatherForecastsCached.ToList();
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed);
        stopwatch = Stopwatch.StartNew();
        data = dbContext.WeatherForecastsCached.ToList();
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed);
        return Ok();
    }

    [HttpPost("fill")]
    public async Task<IActionResult> FillMock()
    {
        ConcurrentBag<WeatherForecast2> weatherForecasts = [];

        // parallel 100 threads add data to weatherForecasts 1000 time
        for (int i = 0; i < 100; i++)
        {
            Parallel.For(0, 9000, i =>
            {
                weatherForecasts.Add(WeatherForecast2.Random());
            });
        }

        await dbContext.WeatherForecastsCached.AddRangeAsync(weatherForecasts);

        return Ok();
    }

    ConcurrentBag<StressModel> results = [];

    [HttpGet("Stress")]
    public async Task<IActionResult> Stress()
    {
        for (int i = 0; i < 1; i++)
        {
            Parallel.For(0, 50, i =>
            {
                Random random = new();
                var method = random.Next(0, 100);
                if (method % 3 == 0)
                {
                    ReadData();
                }
                else
                {
                    WriteData();
                }
            });
        }

        //calculate average read and write
        List<double> reads = [];
        List<double> writes = [];
        foreach (var item in results)
        {
            if (item.methodName == "ReadData")
            {
                reads.Add(item.milliseconds);
            }
            else
            {
                writes.Add(item.milliseconds);
            }
        }
        string result = $"Reads: {reads.Average()}\n\nMin: {reads.Min()}\n\nMax: {reads.Max()}";
        Console.WriteLine(result);
        await Task.Delay(1);
        results.SaveAsExcelFile("StressTest.xlsx");
        return Ok();
    }

    private void ReadData()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var data = dbContext.WeatherForecastsCached.ToList();
        stopwatch.Stop();
        results.Add(new StressModel("ReadData", stopwatch.Elapsed.TotalMilliseconds));
    }

    private void WriteData()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        dbContext.WeatherForecastsCached.Add(WeatherForecast2.Random());
        stopwatch.Stop();
        results.Add(new StressModel("WriteData", stopwatch.Elapsed.TotalMilliseconds));
    }
}

public record StressModel(string methodName, double milliseconds);