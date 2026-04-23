using Posts.Application;
using Posts.Application.Commands;
using Posts.Application.Ports;
using Posts.Domain.Abstractions;
using Xunit;

namespace Posts.UnitTests;

public sealed class UpdatePostCommandHandlerUnitTests
{
    [Fact]
    public async Task HandleThrowsWhenPostNotFound()
    {
        var repository = new StubPostReadRepository(post: null);
        var eventStore = new StubPostEventStore();
        var handler = new UpdatePostCommandHandler(eventStore, repository);

        var exception = await Assert.ThrowsAsync<PostNotFoundException>(
            () => handler.HandleAsync(
                new UpdatePostCommand(Guid.NewGuid(), "New Title", "New Description", "New Content"),
                CancellationToken.None));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task HandleThrowsWhenPostIsDeleted()
    {
        var postId = Guid.NewGuid();
        var deletedPost = new PostReadModel(postId, Guid.NewGuid(), "Title", "Description", "Content", IsDeleted: true);
        var repository = new StubPostReadRepository(deletedPost);
        var eventStore = new StubPostEventStore();
        var handler = new UpdatePostCommandHandler(eventStore, repository);

        var exception = await Assert.ThrowsAsync<PostDeletedException>(
            () => handler.HandleAsync(
                new UpdatePostCommand(postId, "New Title", "New Description", "New Content"),
                CancellationToken.None));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task HandleUpdatesPostAndEmitsEvent()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = new PostReadModel(postId, authorId, "Title", "Description", "Content", IsDeleted: false);
        var repository = new StubPostReadRepository(post);
        var eventStore = new StubPostEventStore();
        var handler = new UpdatePostCommandHandler(eventStore, repository);

        var result = await handler.HandleAsync(
            new UpdatePostCommand(postId, "New Title", "New Description", "New Content"),
            CancellationToken.None);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Description", result.Description);
        Assert.Equal("New Content", result.Content);
        Assert.NotEmpty(eventStore.StoredEvents);
    }

    private sealed class StubPostEventStore : IPostEventStore
    {
        public List<IDomainEvent> StoredEvents { get; } = [];

        public Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            StoredEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class StubPostReadRepository : IPostReadRepository
    {
        private readonly PostReadModel? post;

        public StubPostReadRepository(PostReadModel? post = null)
        {
            this.post = post;
        }

        public Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(post);

        public Task<IReadOnlyList<PostReadModel>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<PostReadModel>)[]);

        public Task SaveAsync(PostReadModel post, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}


