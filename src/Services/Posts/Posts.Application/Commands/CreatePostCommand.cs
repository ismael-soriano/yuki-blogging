namespace Posts.Application.Commands;

public sealed record CreatePostCommand(Guid AuthorId, string Title, string Description, string Content);
