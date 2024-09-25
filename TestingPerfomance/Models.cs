namespace TestingPerfomance;

public class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string GenreId { get; set; } = string.Empty;
}