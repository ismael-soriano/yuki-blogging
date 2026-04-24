using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Domain.Aggregates;
using Posts.Domain.Events;

namespace Posts.Application.Commands;

public sealed class UpdatePostCommandHandler
{
    private readonly IPostEventStore postEventStore;
    private readonly IPostReadRepository postReadRepository;

    public UpdatePostCommandHandler(IPostEventStore postEventStore, IPostReadRepository postReadRepository)
    {
        this.postEventStore = postEventStore;
        this.postReadRepository = postReadRepository;
    }

    public async Task<PostResponse> HandleAsync(UpdatePostCommand command, CancellationToken cancellationToken)
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

        // Reconstruct aggregate from read model and apply update to emit PostUpdatedEvent
        var aggregate = PostAggregate.Reconstruct(command.PostId, post.AuthorId, post.Title, post.Description, post.Content, post.IsDeleted);
        aggregate.Update(command.Title, command.Description, command.Content);

        var domainEvents = aggregate.DequeueUncommittedEvents();
        await postEventStore.AppendAsync(command.PostId, domainEvents, cancellationToken);

        // Update read model from event
        var updatedEvent = domainEvents.OfType<PostUpdatedEvent>().Single();
        var updatedPost = new PostReadModel(
            updatedEvent.PostId,
            post.AuthorId,
            updatedEvent.Title,
            updatedEvent.Description,
            updatedEvent.Content,
            post.IsDeleted);

        await postReadRepository.UpdateAsync(updatedPost, cancellationToken);

        return new PostResponse(updatedPost.Id, updatedPost.Title, updatedPost.Description, updatedPost.Content, null);
    }
}

public sealed class PostNotFoundException : Exception
{
    public PostNotFoundException(Guid postId)
        : base($"Post with id '{postId}' not found.")
    {
    }
}

public sealed class PostDeletedException : Exception
{
    public PostDeletedException(Guid postId)
        : base($"Post with id '{postId}' has been deleted.")
    {
    }
}

