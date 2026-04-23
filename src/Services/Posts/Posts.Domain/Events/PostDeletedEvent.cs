using Posts.Domain.Abstractions;

namespace Posts.Domain.Events;

public sealed record PostDeletedEvent(
    Guid PostId,
    DateTimeOffset OccurredOn) : IDomainEvent;

