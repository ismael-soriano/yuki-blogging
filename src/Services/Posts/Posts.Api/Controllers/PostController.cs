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
    private readonly UpdatePostCommandHandler updatePostCommandHandler;
    private readonly DeletePostCommandHandler deletePostCommandHandler;
    private readonly GetPostByIdQueryHandler getPostByIdQueryHandler;
    private readonly GetAllPostsQueryHandler getAllPostsQueryHandler;

    public PostController(
        CreatePostCommandHandler createPostCommandHandler,
        UpdatePostCommandHandler updatePostCommandHandler,
        DeletePostCommandHandler deletePostCommandHandler,
        GetPostByIdQueryHandler getPostByIdQueryHandler,
        GetAllPostsQueryHandler getAllPostsQueryHandler)
    {
        this.createPostCommandHandler = createPostCommandHandler;
        this.updatePostCommandHandler = updatePostCommandHandler;
        this.deletePostCommandHandler = deletePostCommandHandler;
        this.getPostByIdQueryHandler = getPostByIdQueryHandler;
        this.getAllPostsQueryHandler = getAllPostsQueryHandler;
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
                [nameof(request.AuthorId)] = [exception.Message]
            }));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["post"] = [exception.Message]
            }));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, [FromQuery] bool includeAuthor = false, CancellationToken cancellationToken = default)
    {
        var post = await getPostByIdQueryHandler.HandleAsync(new GetPostByIdQuery(id, includeAuthor), cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] GetAllPostsHttpRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var posts = await getAllPostsQueryHandler.HandleAsync(new GetAllPostsQuery(request.Page, request.PageSize, request.IncludeAuthor), cancellationToken);
            return Ok(posts);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["query"] = [exception.Message]
            }));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePostHttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await updatePostCommandHandler.HandleAsync(
                new UpdatePostCommand(id, request.Title, request.Description, request.Content),
                cancellationToken);

            return Ok(post);
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (PostDeletedException)
        {
            return Conflict(new ProblemDetails { Title = "Post Deleted", Detail = "Cannot update a deleted post." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["post"] = [exception.Message]
            }));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await deletePostCommandHandler.HandleAsync(new DeletePostCommand(id), cancellationToken);
            return NoContent();
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (PostDeletedException)
        {
            return Conflict(new ProblemDetails { Title = "Post Deleted", Detail = "Post is already deleted." });
        }
    }
}
