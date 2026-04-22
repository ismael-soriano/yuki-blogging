using Authors.Application.Ports;
using Authors.Domain.Entities;

namespace Authors.Infrastructure.Persistence;

public sealed class InMemoryAuthorRepository : IAuthorRepository
{
    private readonly IReadOnlyDictionary<Guid, Author> authors = new Dictionary<Guid, Author>
    {
        [AuthorSeedData.AdaId] = new(AuthorSeedData.AdaId, "Ada", "Lovelace"),
        [AuthorSeedData.LinusId] = new(AuthorSeedData.LinusId, "Linus", "Torvalds")
    };

    public Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        authors.TryGetValue(id, out var author);
        return Task.FromResult(author);
    }
}
