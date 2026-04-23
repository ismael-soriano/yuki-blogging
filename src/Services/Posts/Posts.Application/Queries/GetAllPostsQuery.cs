namespace Posts.Application.Queries;

public sealed record GetAllPostsQuery(int Page = 1, int PageSize = 10, bool IncludeAuthor = false);

