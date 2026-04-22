using Authors.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Authors.Api.Controllers;

[ApiController]
[Route("authors")]
[Produces("application/json")]
public sealed class AuthorsController : ControllerBase
{
    private readonly GetAuthorByIdQueryHandler getAuthorByIdQueryHandler;

    public AuthorsController(GetAuthorByIdQueryHandler getAuthorByIdQueryHandler)
    {
        this.getAuthorByIdQueryHandler = getAuthorByIdQueryHandler;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await getAuthorByIdQueryHandler.HandleAsync(new GetAuthorByIdQuery(id), cancellationToken);

        return author is null ? NotFound() : Ok(author);
    }
}
