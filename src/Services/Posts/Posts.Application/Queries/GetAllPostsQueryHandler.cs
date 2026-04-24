using Posts.Application.Common;
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

    public async Task<PagedResult<PostResponse>> HandleAsync(GetAllPostsQuery query, CancellationToken cancellationToken)
    {
        var (posts, totalCount) = await postReadRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        var results = new List<PostResponse>(posts.Count);

        foreach (var post in posts)
        {
            var author = query.IncludeAuthor
                ? await authorDirectory.GetByIdAsync(post.AuthorId, cancellationToken)
                : null;

            results.Add(new PostResponse(post.Id, post.Title, post.Description, post.Content, author));
        }

        return new PagedResult<PostResponse>
        {
            Items = results,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
        };
    }
}

