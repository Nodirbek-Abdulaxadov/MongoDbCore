using Xunit;
using Microsoft.Extensions.Configuration;

namespace MongoDbCore.Tests;

public class MongoDbCoreOptionsTests
{
    [Fact]
    public void Defaults_are_safe_for_local()
    {
        var o = new MongoDbCoreOptions();
        Assert.Equal("mongodb://localhost:27017", o.Connection);
        Assert.Equal("test", o.Database);
        Assert.Equal(100, o.MaxConnectionPoolSize);
        Assert.False(o.EnableAuditing);
    }

    [Fact]
    public void Constructor_assigns_all_fields()
    {
        var o = new MongoDbCoreOptions("mongodb://example:1", "db1", 50, enableAuditing: true);
        Assert.Equal("mongodb://example:1", o.Connection);
        Assert.Equal("db1", o.Database);
        Assert.Equal(50, o.MaxConnectionPoolSize);
        Assert.True(o.EnableAuditing);
    }

    [Fact]
    public void Binds_from_IConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDB:Connection"] = "mongodb://configured:27017",
                ["MongoDB:Database"] = "configured-db",
                ["MongoDB:MaxConnectionPoolSize"] = "42",
                ["MongoDB:EnableAuditing"] = "true",
            })
            .Build();

        var o = config.GetSection("MongoDB").Get<MongoDbCoreOptions>();
        Assert.NotNull(o);
        Assert.Equal("mongodb://configured:27017", o!.Connection);
        Assert.Equal("configured-db", o.Database);
        Assert.Equal(42, o.MaxConnectionPoolSize);
        Assert.True(o.EnableAuditing);
    }
}
