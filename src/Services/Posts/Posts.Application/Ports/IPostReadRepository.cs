namespace Posts.Application.Ports;

public interface IPostReadRepository
{
    Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IReadOnlyList<PostReadModel> Items, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    Task SaveAsync(PostReadModel post, CancellationToken cancellationToken);

    Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
