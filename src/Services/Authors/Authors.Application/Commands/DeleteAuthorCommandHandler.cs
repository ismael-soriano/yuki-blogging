using Authors.Application.Ports;

namespace Authors.Application.Commands;

public sealed class DeleteAuthorCommandHandler
{
    private readonly IAuthorRepository authorRepository;

    public DeleteAuthorCommandHandler(IAuthorRepository authorRepository)
    {
        this.authorRepository = authorRepository;
    }

    public Task<bool> HandleAsync(DeleteAuthorCommand command, CancellationToken cancellationToken)
    {
        return authorRepository.DeleteAsync(command.AuthorId, cancellationToken);
    }
}

