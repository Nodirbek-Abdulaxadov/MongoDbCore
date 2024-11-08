namespace TestingPerfomance;

public class Postgres_AppDbContext : DbContext
{
    public Postgres_AppDbContext()
    {
        Database.EnsureCreated();
    }

    public DbSet<Book> Books { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=TestingPerfomance;Username=postgres;Password=1234");
    }
}