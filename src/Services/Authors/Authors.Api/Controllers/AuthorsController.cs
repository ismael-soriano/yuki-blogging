using Authors.Api.Contracts;
using Authors.Application.Commands;
using Authors.Application.Contracts;
using Authors.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Authors.Api.Controllers;

[ApiController]
[Route("authors")]
[Produces("application/json")]
public sealed class AuthorsController : ControllerBase
{
    private readonly CreateAuthorCommandHandler createAuthorCommandHandler;
    private readonly UpdateAuthorCommandHandler updateAuthorCommandHandler;
    private readonly DeleteAuthorCommandHandler deleteAuthorCommandHandler;
    private readonly GetAuthorsQueryHandler getAuthorsQueryHandler;
    private readonly GetAuthorByIdQueryHandler getAuthorByIdQueryHandler;

    public AuthorsController(
        CreateAuthorCommandHandler createAuthorCommandHandler,
        UpdateAuthorCommandHandler updateAuthorCommandHandler,
        DeleteAuthorCommandHandler deleteAuthorCommandHandler,
        GetAuthorsQueryHandler getAuthorsQueryHandler,
        GetAuthorByIdQueryHandler getAuthorByIdQueryHandler)
    {
        this.createAuthorCommandHandler = createAuthorCommandHandler;
        this.updateAuthorCommandHandler = updateAuthorCommandHandler;
        this.deleteAuthorCommandHandler = deleteAuthorCommandHandler;
        this.getAuthorsQueryHandler = getAuthorsQueryHandler;
        this.getAuthorByIdQueryHandler = getAuthorByIdQueryHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AuthorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AuthorResponse>>> Get(CancellationToken cancellationToken)
    {
        var authors = await getAuthorsQueryHandler.HandleAsync(new GetAuthorsQuery(), cancellationToken);
        return Ok(authors);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await getAuthorByIdQueryHandler.HandleAsync(new GetAuthorByIdQuery(id), cancellationToken);

        return author is null ? NotFound() : Ok(author);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthorResponse>> Create([FromBody] UpsertAuthorHttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var author = await createAuthorCommandHandler.HandleAsync(
                new CreateAuthorCommand(request.Name, request.Surname),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = author.Id }, author);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["author"] = [exception.Message]
            }));
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AuthorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthorResponse>> Update(Guid id, [FromBody] UpsertAuthorHttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var author = await updateAuthorCommandHandler.HandleAsync(
                new UpdateAuthorCommand(id, request.Name, request.Surname),
                cancellationToken);

            return author is null ? NotFound() : Ok(author);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["author"] = [exception.Message]
            }));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await deleteAuthorCommandHandler.HandleAsync(new DeleteAuthorCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
