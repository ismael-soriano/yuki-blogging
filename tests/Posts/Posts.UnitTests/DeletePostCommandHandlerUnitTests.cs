using Posts.Application;
using Posts.Application.Commands;
using Posts.Application.Ports;
using Posts.Domain.Abstractions;
using Xunit;

namespace Posts.UnitTests;

public sealed class DeletePostCommandHandlerUnitTests
{
    [Fact]
    public async Task HandleThrowsWhenPostNotFound()
    {
        var repository = new StubPostReadRepository(post: null);
        var eventStore = new StubPostEventStore();
        var handler = new DeletePostCommandHandler(eventStore, repository);

        var exception = await Assert.ThrowsAsync<PostNotFoundException>(
            () => handler.HandleAsync(
                new DeletePostCommand(Guid.NewGuid()),
                CancellationToken.None));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task HandleThrowsWhenPostAlreadyDeleted()
    {
        var postId = Guid.NewGuid();
        var deletedPost = new PostReadModel(postId, Guid.NewGuid(), "Title", "Description", "Content", IsDeleted: true);
        var repository = new StubPostReadRepository(deletedPost);
        var eventStore = new StubPostEventStore();
        var handler = new DeletePostCommandHandler(eventStore, repository);

        var exception = await Assert.ThrowsAsync<PostDeletedException>(
            () => handler.HandleAsync(
                new DeletePostCommand(postId),
                CancellationToken.None));

        Assert.NotNull(exception);
    }

    [Fact]
    public async Task HandleDeletesPostAndEmitsEvent()
    {
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var post = new PostReadModel(postId, authorId, "Title", "Description", "Content", IsDeleted: false);
        var repository = new StubPostReadRepository(post);
        var eventStore = new StubPostEventStore();
        var handler = new DeletePostCommandHandler(eventStore, repository);

        await handler.HandleAsync(
            new DeletePostCommand(postId),
            CancellationToken.None);

        Assert.True(repository.Deleted);
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

        public bool Deleted { get; private set; }

        public StubPostReadRepository(PostReadModel? post = null)
        {
            this.post = post;
        }

        public Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(post);

        public Task<(IReadOnlyList<PostReadModel> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default) => Task.FromResult(((IReadOnlyList<PostReadModel>)[], 0));

        public Task SaveAsync(PostReadModel post, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            Deleted = true;
            return Task.CompletedTask;
        }
    }
}



