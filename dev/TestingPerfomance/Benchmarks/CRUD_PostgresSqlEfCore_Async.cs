namespace TestingPerfomance.Benchmarks;

[MemoryDiagnoser]
public class CRUD_Postgres_EFCore_Async
{
    private Postgres_AppDbContext dbContext = new();

    [GlobalSetup]
    public async Task Setup()
    {
        List<Book> books = [];
        for (int i = 0; i < 1; i++)
        {
            books.Add(new Book
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Title = $"Title {i}",
                Year = DateTime.Now.Year - i,
                AuthorId = BaseEntity.NewId,
                GenreId = BaseEntity.NewId
            });
        }
        await dbContext.Books.AddRangeAsync(books);
        await dbContext.SaveChangesAsync();
    }

    [Benchmark]
    public async Task AddBook()
    {
        var book = new Book
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Title = $"Title",
            Year = DateTime.Now.Year,
            AuthorId = BaseEntity.NewId,
            GenreId = BaseEntity.NewId
        };
        await dbContext.Books.AddAsync(book);
        await dbContext.SaveChangesAsync();
    }

    [Benchmark]
    public async Task RetrieveBooks()
    {
        var bookList = await dbContext.Books.ToListAsync();
    }

    [Benchmark]
    public async Task UpdateBook()
    {
        var book = await dbContext.Books.FirstOrDefaultAsync();
        book!.Title += " Updated";
        dbContext.Books.Update(book);
        await dbContext.SaveChangesAsync();
    }

    [Benchmark]
    public async Task DeleteBook()
    {
        var book = await dbContext.Books.FirstOrDefaultAsync();
        dbContext.Books.Remove(book!);
        await dbContext.SaveChangesAsync();
    }
}
