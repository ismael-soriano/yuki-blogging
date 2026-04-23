using Posts.Domain.Abstractions;
using Posts.Domain.Events;

namespace Posts.Domain.Aggregates;

public sealed class PostAggregate
{
    private readonly List<IDomainEvent> uncommittedEvents = [];

    private PostAggregate()
    {
    }

    public Guid Id { get; private set; }

    public Guid AuthorId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }

    public static PostAggregate Create(Guid id, Guid authorId, string title, string description, string content)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Post id is required.", nameof(id));
        }

        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("Author id is required.", nameof(authorId));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required.", nameof(content));
        }

        var aggregate = new PostAggregate();

        aggregate.Apply(new PostCreatedEvent(
            id,
            authorId,
            title.Trim(),
            description.Trim(),
            content.Trim(),
            DateTimeOffset.UtcNow));

        return aggregate;
    }

    public static PostAggregate Reconstruct(Guid id, Guid authorId, string title, string description, string content, bool isDeleted)
    {
        var aggregate = new PostAggregate
        {
            Id = id,
            AuthorId = authorId,
            Title = title,
            Description = description,
            Content = content,
            IsDeleted = isDeleted
        };
        return aggregate;
    }

    public IReadOnlyCollection<IDomainEvent> DequeueUncommittedEvents()
    {
        var events = uncommittedEvents.ToArray();
        uncommittedEvents.Clear();
        return events;
    }

    public void Update(string title, string description, string content)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Cannot update a deleted post.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Content is required.", nameof(content));
        }

        Apply(new PostUpdatedEvent(Id, title.Trim(), description.Trim(), content.Trim(), DateTimeOffset.UtcNow));
    }

    public void Delete()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Post is already deleted.");
        }

        Apply(new PostDeletedEvent(Id, DateTimeOffset.UtcNow));
    }

    private void Apply(PostCreatedEvent domainEvent)
    {
        Id = domainEvent.PostId;
        AuthorId = domainEvent.AuthorId;
        Title = domainEvent.Title;
        Description = domainEvent.Description;
        Content = domainEvent.Content;
        uncommittedEvents.Add(domainEvent);
    }

    private void Apply(PostUpdatedEvent domainEvent)
    {
        Title = domainEvent.Title;
        Description = domainEvent.Description;
        Content = domainEvent.Content;
        uncommittedEvents.Add(domainEvent);
    }

    private void Apply(PostDeletedEvent domainEvent)
    {
        IsDeleted = true;
        uncommittedEvents.Add(domainEvent);
    }
}
