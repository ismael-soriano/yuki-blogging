using Authors.Domain.Entities;

namespace Authors.Application.Ports;

public interface IAuthorRepository
{
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
