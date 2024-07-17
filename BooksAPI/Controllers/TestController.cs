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
        var step1 = dbContext.ClassAs.Include(x => x.ClassB);
        var step2 = step1.ThenInclude<ClassB, ClassC>(x => x.ClassC);
        var classa = step2.ToList();
        return Ok(classa);
    }

}
