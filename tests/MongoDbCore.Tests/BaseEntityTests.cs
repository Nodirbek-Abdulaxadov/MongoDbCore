using Xunit;
using MongoDB.Bson;

namespace MongoDbCore.Tests;

public class BaseEntityTests
{
    private sealed class Sample : BaseEntity { }

    [Fact]
    public void NewEntity_has_valid_ObjectId()
    {
        var s = new Sample();
        Assert.True(ObjectId.TryParse(s.Id, out _));
    }

    [Fact]
    public void NewEntity_initializes_timestamps_in_UTC()
    {
        var s = new Sample();
        Assert.Equal(TimeSpan.Zero, s.CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, s.UpdatedAt.Offset);
    }

    [Fact]
    public void Equals_compares_by_id()
    {
        var a = new Sample();
        var b = new Sample { Id = a.Id };
        var c = new Sample();
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }

    [Fact]
    public void GetHashCode_uses_id()
    {
        var a = new Sample();
        var b = new Sample { Id = a.Id };
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void NewId_returns_fresh_ObjectId_each_call()
    {
        var a = BaseEntity.NewId;
        var b = BaseEntity.NewId;
        Assert.NotEqual(a, b);
        Assert.True(ObjectId.TryParse(a, out _));
    }
}
