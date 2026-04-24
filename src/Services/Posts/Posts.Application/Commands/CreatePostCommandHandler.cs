using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Domain.Aggregates;
using Posts.Domain.Events;

namespace Posts.Application.Commands;

public sealed class CreatePostCommandHandler
{
    private readonly IAuthorDirectory authorDirectory;
    private readonly IPostEventStore postEventStore;
    private readonly IPostReadRepository postReadRepository;

    public CreatePostCommandHandler(
        IAuthorDirectory authorDirectory,
        IPostEventStore postEventStore,
        IPostReadRepository postReadRepository)
    {
        this.authorDirectory = authorDirectory;
        this.postEventStore = postEventStore;
        this.postReadRepository = postReadRepository;
    }

    public async Task<PostResponse> HandleAsync(CreatePostCommand command, CancellationToken cancellationToken)
    {
        if (!await authorDirectory.AuthorExistsAsync(command.AuthorId, cancellationToken))
        {
            throw new AuthorNotFoundException(command.AuthorId);
        }

        var aggregate = PostAggregate.Create(Guid.NewGuid(), command.AuthorId, command.Title, command.Description, command.Content);
        var domainEvents = aggregate.DequeueUncommittedEvents();

        await postEventStore.AppendAsync(aggregate.Id, domainEvents, cancellationToken);

        var createdEvent = domainEvents.OfType<PostCreatedEvent>().Single();
        await postReadRepository.SaveAsync(
            new PostReadModel(
                createdEvent.PostId,
                createdEvent.AuthorId,
                createdEvent.Title,
                createdEvent.Description,
                createdEvent.Content),
            cancellationToken);

        return new PostResponse(
            createdEvent.PostId,
            createdEvent.Title,
            createdEvent.Description,
            createdEvent.Content,
            null);
    }
}
