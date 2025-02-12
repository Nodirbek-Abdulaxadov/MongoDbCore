/*namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var list = await dbContext.WeatherForecasts.ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
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
            var result = await dbContext.WeatherForecasts.AddAsync(model);
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

    [HttpDelete("{id}")]
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
}*/