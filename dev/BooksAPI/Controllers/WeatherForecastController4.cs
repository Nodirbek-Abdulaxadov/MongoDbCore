using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Diagnostics;
using WebApplication1.Data.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/test")]
public class WeatherForecastController4(AppDbContext dbContext) : ControllerBase
{

    [HttpPost("fill")]
    public async Task<IActionResult> Fill()
    {
        var weathers = dbContext.WeatherForecastsCached.ToList();
        ConcurrentBag<CityEntity> cities = [];
        ConcurrentBag<Country> countries = [];

        for (int i = 0; i < 10; i++)
        {
            Parallel.For(i, 100, (i) =>
            {
                countries.Add(Country.Random());
            });
        }
        dbContext.Countries.AddRange(countries);

        for (int i = 0; i < 100; i++)
        {
            Parallel.For(i, 100, (i) =>
            {
                Random random = new();
                var index = random.Next(0, countries.Count - 1);
                var randomCountry = countries.ElementAt(index);
                index = random.Next(0, weathers.Count - 1);
                var randomWeather = weathers.ElementAt(index);

                CityEntity city = new()
                {
                    Name = Guid.NewGuid().ToString(),
                    CountryId = randomCountry.Id,
                    WeatherId = randomWeather.Id
                };
                cities.Add(city);
            });
        }

        dbContext.Cities.AddRange(cities);

        return Ok($"""
        Weathers count: {dbContext.WeatherForecastsCached.Count()}
        Countries count: {dbContext.Countries.Count()}
        Cities count: {dbContext.Cities.Count()}
        """);
    }

    [HttpGet("cities")]
    public async Task<IActionResult> Get()
    {
        Random r = new();
        int takeCount = r.Next(1, 100);
        var list = dbContext.Cities.Take(takeCount).ToList();
        return Ok(list);
    }

    [HttpGet("cities-weathers")]
    public async Task<IActionResult> GetCityWeatherData()
    {
        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var result = await dbContext.Cities.Source!
                .Aggregate()
                .AppendStage<BsonDocument>(new BsonDocument("$addFields", new BsonDocument
                {
                    { "WeatherId", new BsonDocument("$toObjectId", "$WeatherId") }, // Ensure WeatherId is an ObjectId
                    { "CountryId", new BsonDocument("$toObjectId", "$CountryId") } // Ensure WeatherId is an ObjectId
                }
                ))
                .Lookup(
                    foreignCollectionName: "weather_forecast2s",
                    localField: "WeatherId",
                    foreignField: "_id",
                    @as: "WeatherForecast")
                .Unwind("WeatherForecast", new AggregateUnwindOptions<BsonDocument>
                {
                    PreserveNullAndEmptyArrays = true
                })
                .Lookup(
                    foreignCollectionName: "countries",
                    localField: "CountryId",
                    foreignField: "_id",
                    @as: "Country")
                .Unwind("Country", new AggregateUnwindOptions<BsonDocument>
                {
                    PreserveNullAndEmptyArrays = true
                })
                .Limit(150)
                .ToListAsync();

            stopwatch.Stop();
            Console.WriteLine("MongoTime: " + stopwatch.Elapsed.TotalMilliseconds);

            stopwatch = Stopwatch.StartNew();
            // Map to DTO for structured output
            var mappedResult = result.Select(doc => new CityEntity
            {
                Name = doc["Name"].AsString,
                CreatedAt = doc.Contains("CreatedAt") && doc["CreatedAt"].IsBsonDateTime
                    ? GetValidDateTime(doc["CreatedAt"].ToUniversalTime())
                    : DateTimeOffset.MinValue, // Fallback for missing or invalid dates
                WeatherForecast = doc.Contains("WeatherForecast") && !doc["WeatherForecast"].IsBsonNull
                    ? new WeatherForecast2
                    {
                        Id = doc["WeatherForecast"]["_id"].AsObjectId.ToString(),
                        CreatedAt = doc.Contains("CreatedAt") && doc["WeatherForecast"]["CreatedAt"].IsBsonDateTime
                                    ? GetValidDateTime(doc["WeatherForecast"]["CreatedAt"].ToUniversalTime())
                                    : DateTimeOffset.MinValue, // Fallback for missing or invalid dates
                        TemperatureC = doc["WeatherForecast"]["TemperatureC"].AsInt32,
                        Summary = doc["WeatherForecast"]["Summary"].AsString
                    }
                    : null,
                Country = doc.Contains("Country") && !doc["Country"].IsBsonNull
                    ? new Country
                    {
                        Id = doc["Country"]["_id"].AsObjectId.ToString(),
                        CreatedAt = doc.Contains("CreatedAt") && doc["Country"]["CreatedAt"].IsBsonDateTime
                                    ? GetValidDateTime(doc["Country"]["CreatedAt"].ToUniversalTime())
                                    : DateTimeOffset.MinValue, // Fallback for missing or invalid dates
                        Name = doc["Country"]["Name"].AsString
                    }
                    : null
            });

            stopwatch.Stop();
            Console.WriteLine("MapTime: " + stopwatch.Elapsed.TotalMilliseconds);


            return Ok(mappedResult); // Limit response to 10 items for simplicity
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching data: {ex.Message}");
            return StatusCode(500, "An error occurred while fetching city weather data.");
        }
    }

    [HttpGet("cities-weathers-include")]
    public async Task<IActionResult> GetCityWeatherDataInclude()
    {
        try
        {
            var result = dbContext.Cities.IncludeRef(x => x.WeatherForecast!).ToList();

            return Ok(result); // Limit response to 10 items for simplicity
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching data: {ex.Message}");
            return StatusCode(500, "An error occurred while fetching city weather data.");
        }
    }

    private DateTimeOffset GetValidDateTime(DateTimeOffset dateTime)
    {
        if (dateTime < DateTimeOffset.MinValue || dateTime > DateTimeOffset.MaxValue)
        {
            // Replace with a fallback value if out of range
            return DateTimeOffset.MinValue;
        }

        return dateTime;
    }
}