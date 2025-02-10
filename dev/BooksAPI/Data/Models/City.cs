namespace WebApplication1.Data.Models;

public class CityEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    [ReferenceTo(nameof(WeatherForecast2))]  
    public string WeatherId { get; set; } = NewId;
    public WeatherForecast2? WeatherForecast { get; set; }
    [ReferenceTo(nameof(Country))]
    public string CountryId { get; set; } = NewId;
    public Country? Country { get; set; }
}