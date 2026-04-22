using Authors.Domain.Entities;

namespace Authors.Application.Ports;

public interface IAuthorRepository
{
    Task<IReadOnlyCollection<Author>> GetAllAsync(CancellationToken cancellationToken);
    Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Author author, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Author author, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
