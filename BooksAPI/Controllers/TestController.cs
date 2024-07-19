using System.Diagnostics;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet("dsss")]
    public IActionResult Gets()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        var fod = dbContext.ClassAs.FirstOrDefault();
        var fodwv = dbContext.ClassAs.FirstOrDefault(x => x.Number > 900);
        var tl = dbContext.ClassAs.ToList();
        var tlwv = dbContext.ClassAs.Where(x => x.Number > 900).ToList();
        var ifod = dbContext.ClassAs.Include(x => x.ClassB).FirstOrDefault();
        var ifodwv = dbContext.ClassAs.Include(x => x.ClassB).FirstOrDefault(x => x.Number > 100);
        var iwtl = dbContext.ClassAs.Where(x => x.Number > 100).Include(x => x.ClassB).ThenInclude(x => x.ClassC).Include(x => x.ClassBList).ToList();

        var res = dbContext.ClassAs.Include(x => x.ClassB).ThenInclude(x => x.ClassC).Include(x => x.ClassBList).ToList();
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed.ToString());



        return Ok(res);
    }

}
