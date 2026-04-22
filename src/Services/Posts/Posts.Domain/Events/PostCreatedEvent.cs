using Posts.Domain.Abstractions;

namespace Posts.Domain.Events;

public sealed record PostCreatedEvent(
    Guid PostId,
    Guid AuthorId,
    string Title,
    string Description,
    string Content,
    DateTimeOffset OccurredOn) : IDomainEvent;
