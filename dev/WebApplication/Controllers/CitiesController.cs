[Route("api/[controller]")]
[ApiController]
public class CitiesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var items = dbContext.Cities.ToList();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        var item = dbContext.Cities.FirstOrDefault(x => x.Id == id);
        return Ok(item);
    }

    [HttpPost]
    public IActionResult Post([FromBody] CreateCity entity)
    {
        var item = new CityEntity { Name = entity.Name, CountryId = entity.CountryId, WeatherId = entity.WeatherId };
        item = dbContext.Cities.Add(item);
        return Ok(item);
    }


    [HttpGet("releation-level1")]
    public IActionResult GetWithReleations1()
    {
        var items = dbContext.Cities.IncludeRef(x => x.WeatherForecast!).ToList();
        return Ok(items);
    }
}