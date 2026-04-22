using Posts.Domain.Aggregates;
using Posts.Domain.Events;
using Xunit;

namespace Posts.UnitTests;

public sealed class PostAggregateUnitTests
{
    [Fact]
    public void CreateProducesPostCreatedEvent()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");

        var domainEvent = Assert.Single(aggregate.DequeueUncommittedEvents());

        Assert.IsType<PostCreatedEvent>(domainEvent);
    }

    [Fact]
    public void CreateRejectsEmptyTitle()
    {
        var exception = Assert.Throws<ArgumentException>(() => PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), string.Empty, "Description", "Content"));

        Assert.Equal("title", exception.ParamName);
    }
}
