using Posts.Domain.Abstractions;

namespace Posts.Domain.Events;

public sealed record PostUpdatedEvent(
    Guid PostId,
    string Title,
    string Description,
    string Content,
    DateTimeOffset OccurredOn) : IDomainEvent;

