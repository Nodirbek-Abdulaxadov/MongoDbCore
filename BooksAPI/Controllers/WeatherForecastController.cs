using System.Diagnostics;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await dbContext.WeatherForecasts.ToListAsync());

    [HttpGet("get")]
    public IActionResult Gett()
    {
        var result = dbContext.WeatherForecasts.Get<Type2>();
        return Ok(result);
    }

    [HttpGet("id")]
    public async Task<IActionResult> Get(string id)
    {
        var model = await dbContext.WeatherForecasts.FirstOrDefaultAsync(x => x.Id == id);
        if (model == null)
        {
            return NotFound();
        }

        return Ok(model);
    }

    [HttpPost]
    public async Task<IActionResult> Post(CreateWeatherForecast model)
    {
        try
        {
            var entity = (WeatherForecast)model;
            Stopwatch stopwatch1 = new();
            Stopwatch stopwatch2 = new();

            stopwatch1.Start();
            if (dbContext.WeatherForecasts.Any(x => x.Date == entity.Date)) 
            { }
            stopwatch1.Stop();

            stopwatch2.Start();
            if (dbContext.WeatherForecasts.Exists(entity, x => x.Date)) 
            { }
            stopwatch2.Stop();

            var time1 = stopwatch1.Elapsed.TotalMilliseconds;
            var time2 = stopwatch2.Elapsed.TotalMilliseconds;

            var result = await dbContext.WeatherForecasts.AddAsync(entity);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> Put(UpdateWeatherForecast model)
    {
        try
        {
            var excitingModel = await dbContext.WeatherForecasts.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (excitingModel == null)
            {
                return NotFound();
            }

            excitingModel = model;

            var result = await dbContext.WeatherForecasts.UpdateAsync((WeatherForecast)excitingModel);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("id")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var model = await dbContext.WeatherForecasts.FirstOrDefaultAsync(x => x.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            await dbContext.WeatherForecasts.DeleteAsync(model);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}