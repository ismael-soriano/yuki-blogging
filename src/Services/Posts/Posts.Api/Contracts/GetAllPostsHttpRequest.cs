namespace Posts.Api.Contracts;

public sealed record GetAllPostsHttpRequest(int Page = 1, int PageSize = 10, bool IncludeAuthor = false);

