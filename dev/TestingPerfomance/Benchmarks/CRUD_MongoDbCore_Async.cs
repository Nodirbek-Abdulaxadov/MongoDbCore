namespace TestingPerfomance.Benchmarks;

[MemoryDiagnoser]
public class CRUD_MongoDbCore_Async
{
    private Mongo_AppDbContext dbContext = new();

    [GlobalSetup]
    public async Task Setup()
    {
        dbContext.Books = new Collection<Book>(dbContext);
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
        
        book.Title += " Updated";
        await dbContext.Books.UpdateAsync(book);
    }

    [Benchmark]
    public async Task DeleteBook()
    {
        var book = await dbContext.Books.FirstOrDefaultAsync();
        await dbContext.Books.DeleteAsync(book);
    }
}