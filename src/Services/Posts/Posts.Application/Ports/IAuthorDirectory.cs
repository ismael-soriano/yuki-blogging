using Posts.Application.Contracts;

namespace Posts.Application.Ports;

public interface IAuthorDirectory
{
    Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken);

    Task<AuthorSummaryResponse?> GetByIdAsync(Guid authorId, CancellationToken cancellationToken);
}
