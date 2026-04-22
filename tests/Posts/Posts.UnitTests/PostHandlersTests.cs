using Posts.Application;
using Posts.Application.Commands;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Application.Queries;
using Posts.Domain.Abstractions;
using Xunit;

namespace Posts.UnitTests;

public sealed class PostHandlersTests
{
    [Fact]
    public async Task CreateHandlerStoresEventAndReadModel()
    {
        var authorDirectory = new FakeAuthorDirectory(exists: true);
        var eventStore = new FakePostEventStore();
        var readRepository = new FakePostReadRepository();
        var handler = new CreatePostCommandHandler(authorDirectory, eventStore, readRepository);

        var result = await handler.HandleAsync(
            new CreatePostCommand(Guid.NewGuid(), "Title", "Description", "Content"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Single(eventStore.StoredEvents);
        Assert.NotNull(await readRepository.GetByIdAsync(result.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateHandlerRejectsUnknownAuthor()
    {
        var handler = new CreatePostCommandHandler(new FakeAuthorDirectory(exists: false), new FakePostEventStore(), new FakePostReadRepository());

        await Assert.ThrowsAsync<AuthorNotFoundException>(
            () => handler.HandleAsync(new CreatePostCommand(Guid.NewGuid(), "Title", "Description", "Content"), CancellationToken.None));
    }

    [Fact]
    public async Task GetQueryIncludesAuthorWhenRequested()
    {
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var authorDirectory = new FakeAuthorDirectory(exists: true)
        {
            Author = new AuthorSummaryResponse(authorId, "Ada", "Lovelace")
        };
        var readRepository = new FakePostReadRepository();
        await readRepository.SaveAsync(new PostReadModel(postId, authorId, "Title", "Description", "Content"), CancellationToken.None);

        var handler = new GetPostByIdQueryHandler(authorDirectory, readRepository);
        var result = await handler.HandleAsync(new GetPostByIdQuery(postId, true), CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.Author);
        Assert.Equal("Ada", result.Author.Name);
    }

    private sealed class FakeAuthorDirectory : IAuthorDirectory
    {
        private readonly bool exists;

        public FakeAuthorDirectory(bool exists)
        {
            this.exists = exists;
        }

        public AuthorSummaryResponse? Author { get; init; }

        public Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken) => Task.FromResult(exists);

        public Task<AuthorSummaryResponse?> GetByIdAsync(Guid authorId, CancellationToken cancellationToken) => Task.FromResult(Author);
    }

    private sealed class FakePostEventStore : IPostEventStore
    {
        public List<IDomainEvent> StoredEvents { get; } = new();

        public Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            StoredEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePostReadRepository : IPostReadRepository
    {
        private readonly Dictionary<Guid, PostReadModel> posts = new();

        public Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            posts.TryGetValue(id, out var post);
            return Task.FromResult(post);
        }

        public Task SaveAsync(PostReadModel post, CancellationToken cancellationToken)
        {
            posts[post.Id] = post;
            return Task.CompletedTask;
        }
    }
}
