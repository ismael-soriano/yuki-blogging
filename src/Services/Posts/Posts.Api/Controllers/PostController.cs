using Microsoft.AspNetCore.Mvc;
using Posts.Api.Contracts;
using Posts.Application.Commands;
using Posts.Application.Queries;

namespace Posts.Api.Controllers;

[ApiController]
[Route("post")]
[Produces("application/json")]
public sealed class PostController : ControllerBase
{
    private readonly CreatePostCommandHandler createPostCommandHandler;
    private readonly GetPostByIdQueryHandler getPostByIdQueryHandler;

    public PostController(
        CreatePostCommandHandler createPostCommandHandler,
        GetPostByIdQueryHandler getPostByIdQueryHandler)
    {
        this.createPostCommandHandler = createPostCommandHandler;
        this.getPostByIdQueryHandler = getPostByIdQueryHandler;
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreatePostHttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await createPostCommandHandler.HandleAsync(
                new CreatePostCommand(request.AuthorId, request.Title, request.Description, request.Content),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = post.Id, includeAuthor = false }, post);
        }
        catch (AuthorNotFoundException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(request.AuthorId)] = new[] { exception.Message }
            }));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["post"] = new[] { exception.Message }
            }));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, [FromQuery] bool includeAuthor = false, CancellationToken cancellationToken = default)
    {
        var post = await getPostByIdQueryHandler.HandleAsync(new GetPostByIdQuery(id, includeAuthor), cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }
}
