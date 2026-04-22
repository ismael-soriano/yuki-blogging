using Authors.Application.Contracts;
using Authors.Application.Ports;
using Authors.Domain.Entities;

namespace Authors.Application.Commands;

public sealed class CreateAuthorCommandHandler
{
    private readonly IAuthorRepository authorRepository;

    public CreateAuthorCommandHandler(IAuthorRepository authorRepository)
    {
        this.authorRepository = authorRepository;
    }

    public async Task<AuthorResponse> HandleAsync(CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        var author = new Author(Guid.NewGuid(), command.Name, command.Surname);
        await authorRepository.AddAsync(author, cancellationToken);
        return new AuthorResponse(author.Id, author.Name, author.Surname);
    }
}

