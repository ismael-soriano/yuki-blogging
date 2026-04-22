using Posts.Application;
using Posts.Application.Ports;

namespace Posts.Infrastructure.Persistence;

public sealed class InMemoryPostReadRepository : IPostReadRepository
{
    private readonly Dictionary<Guid, PostReadModel> posts = new();
    private readonly object sync = new();

    public Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (sync)
        {
            posts.TryGetValue(id, out var post);
            return Task.FromResult(post);
        }
    }

    public Task SaveAsync(PostReadModel post, CancellationToken cancellationToken)
    {
        lock (sync)
        {
            posts[post.Id] = post;
        }

        return Task.CompletedTask;
    }
}
