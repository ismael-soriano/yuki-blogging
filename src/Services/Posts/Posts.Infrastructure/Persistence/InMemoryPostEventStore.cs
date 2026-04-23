using Posts.Application.Ports;
using Posts.Domain.Abstractions;

namespace Posts.Infrastructure.Persistence;

public sealed class InMemoryPostEventStore : IPostEventStore
{
    private readonly Dictionary<Guid, List<IDomainEvent>> streams = new();
    private readonly object sync = new();

    public Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        lock (sync)
        {
            if (!streams.TryGetValue(streamId, out var events))
            {
                events = [];
                streams[streamId] = events;
            }

            events.AddRange(domainEvents);
        }

        return Task.CompletedTask;
    }
}
