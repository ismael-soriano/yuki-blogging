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

    [Fact]
    public void CreateRejectsEmptyDescription()
    {
        var exception = Assert.Throws<ArgumentException>(() => PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", string.Empty, "Content"));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void CreateRejectsEmptyContent()
    {
        var exception = Assert.Throws<ArgumentException>(() => PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", string.Empty));

        Assert.Equal("content", exception.ParamName);
    }

    [Fact]
    public void CreateRejectsEmptyId()
    {
        var exception = Assert.Throws<ArgumentException>(() => PostAggregate.Create(Guid.Empty, Guid.NewGuid(), "Title", "Description", "Content"));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void CreateRejectsEmptyAuthorId()
    {
        var exception = Assert.Throws<ArgumentException>(() => PostAggregate.Create(Guid.NewGuid(), Guid.Empty, "Title", "Description", "Content"));

        Assert.Equal("authorId", exception.ParamName);
    }

    [Fact]
    public void UpdateProducesPostUpdatedEvent()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");
        aggregate.DequeueUncommittedEvents();

        aggregate.Update("New Title", "New Description", "New Content");

        var domainEvent = Assert.Single(aggregate.DequeueUncommittedEvents());
        Assert.IsType<PostUpdatedEvent>(domainEvent);
    }

    [Fact]
    public void UpdateRejectsEmptyTitle()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");

        var exception = Assert.Throws<ArgumentException>(() => aggregate.Update(string.Empty, "New Description", "New Content"));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void UpdateRejectsEmptyDescription()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");

        var exception = Assert.Throws<ArgumentException>(() => aggregate.Update("New Title", string.Empty, "New Content"));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void UpdateRejectsEmptyContent()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");

        var exception = Assert.Throws<ArgumentException>(() => aggregate.Update("New Title", "New Description", string.Empty));

        Assert.Equal("content", exception.ParamName);
    }

    [Fact]
    public void UpdateChangesProperties()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var aggregate = PostAggregate.Create(postId, authorId, "Title", "Description", "Content");
        aggregate.DequeueUncommittedEvents();

        aggregate.Update("New Title", "New Description", "New Content");

        Assert.Equal("New Title", aggregate.Title);
        Assert.Equal("New Description", aggregate.Description);
        Assert.Equal("New Content", aggregate.Content);
        Assert.False(aggregate.IsDeleted);
    }

    [Fact]
    public void DeleteProducesPostDeletedEvent()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");
        aggregate.DequeueUncommittedEvents();

        aggregate.Delete();

        var domainEvent = Assert.Single(aggregate.DequeueUncommittedEvents());
        Assert.IsType<PostDeletedEvent>(domainEvent);
    }

    [Fact]
    public void DeleteSetsIsDeletedFlag()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");

        aggregate.Delete();

        Assert.True(aggregate.IsDeleted);
    }

    [Fact]
    public void CannotUpdateDeletedPost()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");
        aggregate.Delete();

        var exception = Assert.Throws<InvalidOperationException>(() => aggregate.Update("New Title", "New Description", "New Content"));

        Assert.Equal("Cannot update a deleted post.", exception.Message);
    }

    [Fact]
    public void CannotDeleteAlreadyDeletedPost()
    {
        var aggregate = PostAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Description", "Content");
        aggregate.Delete();

        var exception = Assert.Throws<InvalidOperationException>(() => aggregate.Delete());

        Assert.Equal("Post is already deleted.", exception.Message);
    }
}
