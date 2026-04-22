using Authors.Application.Contracts;
using Authors.Application.Ports;

namespace Authors.Application.Queries;

public sealed class GetAuthorsQueryHandler
{
    private readonly IAuthorRepository authorRepository;

    public GetAuthorsQueryHandler(IAuthorRepository authorRepository)
    {
        this.authorRepository = authorRepository;
    }

    public async Task<IReadOnlyCollection<AuthorResponse>> HandleAsync(GetAuthorsQuery query, CancellationToken cancellationToken)
    {
        var authors = await authorRepository.GetAllAsync(cancellationToken);
        return authors
            .Select(author => new AuthorResponse(author.Id, author.Name, author.Surname))
            .ToArray();
    }
}

