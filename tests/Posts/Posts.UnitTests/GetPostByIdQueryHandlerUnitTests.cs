using Posts.Application;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Application.Queries;
using Xunit;

namespace Posts.UnitTests;

public sealed class GetPostByIdQueryHandlerUnitTests
{
    [Fact]
    public async Task HandleAsyncIncludesAuthorWhenRequested()
    {
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var authorDirectory = new FakeAuthorDirectory
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
        public AuthorSummaryResponse? Author { get; init; }

        public Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken) => Task.FromResult(true);

        public Task<AuthorSummaryResponse?> GetByIdAsync(Guid authorId, CancellationToken cancellationToken) => Task.FromResult(Author);
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

