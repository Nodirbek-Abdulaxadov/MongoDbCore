//using MongoDbCore.Identity.Interfaces;

//namespace WebApplication1.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//public class UsersController(IRoleManager roleManager) : ControllerBase
//{
//    [HttpGet]
//    public async Task<IActionResult> Get()
//    {
//        var roles = await roleManager.GetRolesAsync();
//        return Ok(roles);
//    }


//    [HttpGet("{Id}")]
//    public async Task<IActionResult> Get(string id)
//    {
//        var roles = await roleManager.FindByIdAsync(id);
//        return Ok(roles);
//    }

//    [HttpPost]
//    public async Task<IActionResult> Post([FromBody] string role)
//    {
//        await roleManager.CreateAsync(new IdentityRole() { Name = role, NormalizedName = role.ToUpper()});
//        return Ok();
//    }

//    [HttpDelete]
//    public async Task<IActionResult> Delete([FromBody] string id)
//    {
//        var role = await roleManager.FindByIdAsync(id);
//        if (role is null)
//        {
//            return NotFound();
//        }
//        await roleManager.DeleteAsync(role);
//        return Ok();
//    }
//}