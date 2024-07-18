using MongoDB.Driver;
using System.Diagnostics;

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
        Stopwatch stopwatch = Stopwatch.StartNew();
        var res = dbContext.ClassAs.Include(x => x.ClassB).ThenInclude(x => x.ClassC).Include(x => x.ClassBList).ToList();
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed.ToString());



        return Ok(res);
    }

}
