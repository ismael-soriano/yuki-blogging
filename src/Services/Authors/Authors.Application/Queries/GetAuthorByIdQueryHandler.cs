using Authors.Application.Contracts;
using Authors.Application.Ports;

namespace Authors.Application.Queries;

public sealed class GetAuthorByIdQueryHandler
{
    private readonly IAuthorRepository authorRepository;

    public GetAuthorByIdQueryHandler(IAuthorRepository authorRepository)
    {
        this.authorRepository = authorRepository;
    }

    public async Task<AuthorResponse?> HandleAsync(GetAuthorByIdQuery query, CancellationToken cancellationToken)
    {
        var author = await authorRepository.GetByIdAsync(query.AuthorId, cancellationToken);

        return author is null ? null : new AuthorResponse(author.Id, author.Name, author.Surname);
    }
}
