using Xunit;

namespace MongoDbCore.Tests;

public class InflectorTests
{
    [Theory]
    [InlineData("Todo", "Todos")]
    [InlineData("Country", "Countries")]
    [InlineData("City", "Cities")]
    [InlineData("Person", "People")]
    [InlineData("Child", "Children")]
    [InlineData("Goose", "Geese")]
    [InlineData("Fish", "Fish")]      // uncountable
    [InlineData("Sheep", "Sheep")]    // uncountable
    public void Pluralize_returns_expected(string input, string expected)
    {
        Assert.Equal(expected, input.Pluralize());
    }

    [Theory]
    [InlineData("Todo", "todo")]
    [InlineData("TodoItem", "todo_item")]
    [InlineData("URLShortener", "url_shortener")]
    [InlineData("SimpleCase", "simple_case")]
    public void Underscore_returns_snake_case(string input, string expected)
    {
        Assert.Equal(expected, input.Underscore());
    }

    [Theory]
    [InlineData("TodoItem", "todo_items")]
    [InlineData("City", "cities")]
    [InlineData("Country", "countries")]
    public void CollectionName_combines_pluralize_and_underscore(string input, string expected)
    {
        Assert.Equal(expected, input.Pluralize().Underscore());
    }
}
