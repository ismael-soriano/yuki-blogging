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

    public Task<IReadOnlyList<PostReadModel>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        lock (sync)
        {
            var result = posts.Values
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult((IReadOnlyList<PostReadModel>)result);
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

    public Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken)
    {
        lock (sync)
        {
            posts[post.Id] = post;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (sync)
        {
            if (posts.TryGetValue(id, out var post))
            {
                posts[id] = post with { IsDeleted = true };
            }
        }

        return Task.CompletedTask;
    }
}
