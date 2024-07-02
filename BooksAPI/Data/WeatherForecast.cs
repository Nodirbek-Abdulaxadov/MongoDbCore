namespace WebApplication1.Data;

[Cacheable]
public class WeatherForecast : BaseEntity
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}

public class CreateWeatherForecast
{
    public int TemperatureC { get; set; }

    public string? Summary { get; set; }

    public static implicit operator WeatherForecast(CreateWeatherForecast model)
        => new()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = model.TemperatureC,
            Summary = model.Summary
        };
}

public class UpdateWeatherForecast : CreateWeatherForecast
{
    public string Id {  get; set; } = string.Empty;

    public static implicit operator WeatherForecast(UpdateWeatherForecast model)
        => new()
        {
            Id = model.Id,
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = model.TemperatureC,
            Summary = model.Summary
        };
}