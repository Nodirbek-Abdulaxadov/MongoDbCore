namespace WebApplication1.Data.Models;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}