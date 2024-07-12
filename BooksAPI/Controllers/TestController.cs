using MongoDB.Bson;
using MongoDB.Driver;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var res = dbContext.GetCollection<dynamic>("weather_forecasts").Find(FilterDefinition<dynamic>.Empty).ToList();
        return Ok(res);
    }

    [HttpGet("dsss")]
    public IActionResult Gets()
    {
        var classa = dbContext.ClassAs.Include(x => x.ClassCList).Include(x => x.ClassB).ToList();
        return Ok(classa);
    }

}
