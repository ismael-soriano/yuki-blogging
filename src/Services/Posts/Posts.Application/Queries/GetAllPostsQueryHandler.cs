using Posts.Application.Contracts;
using Posts.Application.Ports;

namespace Posts.Application.Queries;

public sealed class GetAllPostsQueryHandler
{
    private readonly IAuthorDirectory authorDirectory;
    private readonly IPostReadRepository postReadRepository;

    public GetAllPostsQueryHandler(IAuthorDirectory authorDirectory, IPostReadRepository postReadRepository)
    {
        this.authorDirectory = authorDirectory;
        this.postReadRepository = postReadRepository;
    }

    public async Task<IReadOnlyList<PostResponse>> HandleAsync(GetAllPostsQuery query, CancellationToken cancellationToken)
    {
        var posts = await postReadRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        var results = new List<PostResponse>(posts.Count);

        foreach (var post in posts)
        {
            // Skip deleted posts in response
            if (post.IsDeleted)
            {
                continue;
            }

            var author = query.IncludeAuthor
                ? await authorDirectory.GetByIdAsync(post.AuthorId, cancellationToken)
                : null;

            results.Add(new PostResponse(post.Id, post.AuthorId, post.Title, post.Description, post.Content, author));
        }

        return results;
    }
}

