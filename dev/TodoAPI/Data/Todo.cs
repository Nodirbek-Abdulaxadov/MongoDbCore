namespace TodoAPI.Data;

public class Todo : BaseEntity
{
    public string Task { get; set; } = string.Empty;
}