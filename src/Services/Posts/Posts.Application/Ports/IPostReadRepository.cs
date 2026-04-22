namespace Posts.Application.Ports;

public interface IPostReadRepository
{
    Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(PostReadModel post, CancellationToken cancellationToken);
}
