using Posts.Application.Ports;
using Posts.Domain.Aggregates;
using Posts.Domain.Events;

namespace Posts.Application.Commands;

public sealed class DeletePostCommandHandler
{
    private readonly IPostEventStore postEventStore;
    private readonly IPostReadRepository postReadRepository;

    public DeletePostCommandHandler(IPostEventStore postEventStore, IPostReadRepository postReadRepository)
    {
        this.postEventStore = postEventStore;
        this.postReadRepository = postReadRepository;
    }

    public async Task HandleAsync(DeletePostCommand command, CancellationToken cancellationToken)
    {
        var post = await postReadRepository.GetByIdAsync(command.PostId, cancellationToken);
        if (post is null)
        {
            throw new PostNotFoundException(command.PostId);
        }

        if (post.IsDeleted)
        {
            throw new PostDeletedException(command.PostId);
        }

        // Reconstruct aggregate and apply delete to emit PostDeletedEvent
        var aggregate = PostAggregate.Reconstruct(command.PostId, post.AuthorId, post.Title, post.Description, post.Content, post.IsDeleted);
        aggregate.Delete();

        var domainEvents = aggregate.DequeueUncommittedEvents();
        await postEventStore.AppendAsync(command.PostId, domainEvents, cancellationToken);

        // Update read model to soft-delete
        await postReadRepository.DeleteAsync(command.PostId, cancellationToken);
    }
}

