using Authors.Api.Contracts;
using Authors.Application.Commands;
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
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        var authors = await getAuthorsQueryHandler.HandleAsync(new GetAuthorsQuery(), cancellationToken);
        return Ok(authors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var author = await getAuthorByIdQueryHandler.HandleAsync(new GetAuthorByIdQuery(id), cancellationToken);

        return author is null ? NotFound() : Ok(author);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] UpsertAuthorHttpRequest request, CancellationToken cancellationToken)
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
                ["author"] = new[] { exception.Message }
            }));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpsertAuthorHttpRequest request, CancellationToken cancellationToken)
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
                ["author"] = new[] { exception.Message }
            }));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await deleteAuthorCommandHandler.HandleAsync(new DeleteAuthorCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
