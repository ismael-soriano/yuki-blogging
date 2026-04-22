using Posts.Domain.Abstractions;

namespace Posts.Application.Ports;

public interface IPostEventStore
{
    Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
