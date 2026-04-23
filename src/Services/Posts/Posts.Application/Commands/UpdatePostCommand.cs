namespace Posts.Application.Commands;

public sealed record UpdatePostCommand(Guid PostId, string Title, string Description, string Content);

