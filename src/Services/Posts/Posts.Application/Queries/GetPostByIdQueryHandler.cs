using Posts.Application.Contracts;
using Posts.Application.Ports;

namespace Posts.Application.Queries;

public sealed class GetPostByIdQueryHandler
{
    private readonly IAuthorDirectory authorDirectory;
    private readonly IPostReadRepository postReadRepository;

    public GetPostByIdQueryHandler(IAuthorDirectory authorDirectory, IPostReadRepository postReadRepository)
    {
        this.authorDirectory = authorDirectory;
        this.postReadRepository = postReadRepository;
    }

    public async Task<PostResponse?> HandleAsync(GetPostByIdQuery query, CancellationToken cancellationToken)
    {
        var post = await postReadRepository.GetByIdAsync(query.PostId, cancellationToken);
        if (post is null)
        {
            return null;
        }

        var author = query.IncludeAuthor
            ? await authorDirectory.GetByIdAsync(post.AuthorId, cancellationToken)
            : null;

        return new PostResponse(post.Id, post.AuthorId, post.Title, post.Description, post.Content, author);
    }
}
