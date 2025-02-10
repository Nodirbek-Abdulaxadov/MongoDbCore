[Route("api/[controller]")]
[ApiController]
public class CountriesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var items = dbContext.Countries.ToList();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public IActionResult Get(string id)
    {
        var item = dbContext.Countries.FirstOrDefault(x => x.Id == id);
        return Ok(item);
    }

    [HttpPost]
    public IActionResult Post([FromBody] string name)
    {
        var item = new Country { Name = name };
        item = dbContext.Countries.Add(item);
        return Ok(item);
    }
}
