using Posts.Application;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Application.Queries;
using Xunit;

namespace Posts.UnitTests;

public sealed class GetAllPostsQueryHandlerUnitTests
{
    [Fact]
    public async Task HandleAsyncReturnsEmptyListWhenNoPostsExist()
    {
        var authorDirectory = new FakeAuthorDirectory();
        var readRepository = new FakePostReadRepository();
        var handler = new GetAllPostsQueryHandler(authorDirectory, readRepository);

        var result = await handler.HandleAsync(new GetAllPostsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsyncReturnsOnlyNonDeletedPosts()
    {
        var authorId = Guid.NewGuid();
        var postId1 = Guid.NewGuid();
        var postId2 = Guid.NewGuid();
        var postId3 = Guid.NewGuid();

        var readRepository = new FakePostReadRepository();
        await readRepository.SaveAsync(new PostReadModel(postId1, authorId, "Title 1", "Description 1", "Content 1", IsDeleted: false), CancellationToken.None);
        await readRepository.SaveAsync(new PostReadModel(postId2, authorId, "Title 2", "Description 2", "Content 2", IsDeleted: true), CancellationToken.None);
        await readRepository.SaveAsync(new PostReadModel(postId3, authorId, "Title 3", "Description 3", "Content 3", IsDeleted: false), CancellationToken.None);

        var authorDirectory = new FakeAuthorDirectory();
        var handler = new GetAllPostsQueryHandler(authorDirectory, readRepository);

        var result = await handler.HandleAsync(new GetAllPostsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, p => p.Id == postId2);
    }

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

        var handler = new GetAllPostsQueryHandler(authorDirectory, readRepository);
        var result = await handler.HandleAsync(new GetAllPostsQuery(IncludeAuthor: true), CancellationToken.None);

        Assert.Single(result);
        var firstResult = result.First();
        Assert.NotNull(firstResult.Author);
        Assert.Equal("Ada", firstResult.Author!.Name);
    }

    [Fact]
    public async Task HandleAsyncRespectsPagination()
    {
        var authorId = Guid.NewGuid();
        var readRepository = new FakePostReadRepository();

        // Create 25 posts
        for (int i = 1; i <= 25; i++)
        {
            await readRepository.SaveAsync(
                new PostReadModel(Guid.NewGuid(), authorId, $"Title {i}", $"Description {i}", $"Content {i}"),
                CancellationToken.None);
        }

        var authorDirectory = new FakeAuthorDirectory();
        var handler = new GetAllPostsQueryHandler(authorDirectory, readRepository);

        // Page 1, size 10
        var page1 = await handler.HandleAsync(new GetAllPostsQuery(Page: 1, PageSize: 10), CancellationToken.None);
        Assert.Equal(10, page1.Count);

        // Page 2, size 10
        var page2 = await handler.HandleAsync(new GetAllPostsQuery(Page: 2, PageSize: 10), CancellationToken.None);
        Assert.Equal(10, page2.Count);

        // Page 3, size 10
        var page3 = await handler.HandleAsync(new GetAllPostsQuery(Page: 3, PageSize: 10), CancellationToken.None);
        Assert.Equal(5, page3.Count);

        // Ensure different posts on each page
        Assert.Empty(page1.Intersect(page2, new PostResponseComparer()));
        Assert.Empty(page2.Intersect(page3, new PostResponseComparer()));
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

        public Task<IReadOnlyList<PostReadModel>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var skip = (page - 1) * pageSize;
            var result = posts.Values
                .OrderByDescending(p => p.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToList();
            return Task.FromResult((IReadOnlyList<PostReadModel>)result);
        }

        public Task SaveAsync(PostReadModel post, CancellationToken cancellationToken)
        {
            posts[post.Id] = post;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken)
        {
            posts[post.Id] = post;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            if (posts.TryGetValue(id, out var post))
            {
                posts[id] = post with { IsDeleted = true };
            }
            return Task.CompletedTask;
        }
    }

    private sealed class PostResponseComparer : IEqualityComparer<PostResponse>
    {
        public bool Equals(PostResponse? x, PostResponse? y) => x?.Id == y?.Id;

        public int GetHashCode(PostResponse obj) => obj.Id.GetHashCode();
    }
}


