using Authors.Application.Ports;
using Authors.Domain.Entities;

namespace Authors.Infrastructure.Persistence;

public sealed class InMemoryAuthorRepository : IAuthorRepository
{
    private readonly Dictionary<Guid, Author> authors = new()
    {
        [AuthorSeedData.AdaId] = new(AuthorSeedData.AdaId, "Ada", "Lovelace"),
        [AuthorSeedData.LinusId] = new(AuthorSeedData.LinusId, "Linus", "Torvalds")
    };

    public Task<IReadOnlyCollection<Author>> GetAllAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<Author>>(authors.Values.ToArray());
    }

    public Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        authors.TryGetValue(id, out var author);
        return Task.FromResult(author);
    }

    public Task AddAsync(Author author, CancellationToken cancellationToken)
    {
        authors[author.Id] = author;
        return Task.CompletedTask;
    }

    public Task<bool> UpdateAsync(Author author, CancellationToken cancellationToken)
    {
        if (!authors.ContainsKey(author.Id))
        {
            return Task.FromResult(false);
        }

        authors[author.Id] = author;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(authors.Remove(id));
    }
}
