namespace Posts.Application.Queries;

public sealed record GetPostByIdQuery(Guid PostId, bool IncludeAuthor);
