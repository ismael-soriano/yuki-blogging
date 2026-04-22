using Authors.Application.Contracts;
using Authors.Application.Ports;
using Authors.Domain.Entities;

namespace Authors.Application.Commands;

public sealed class UpdateAuthorCommandHandler
{
    private readonly IAuthorRepository authorRepository;

    public UpdateAuthorCommandHandler(IAuthorRepository authorRepository)
    {
        this.authorRepository = authorRepository;
    }

    public async Task<AuthorResponse?> HandleAsync(UpdateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = new Author(command.AuthorId, command.Name, command.Surname);
        var updated = await authorRepository.UpdateAsync(author, cancellationToken);
        return updated ? new AuthorResponse(author.Id, author.Name, author.Surname) : null;
    }
}

