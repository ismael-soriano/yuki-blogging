namespace Posts.Application;

public sealed record PostReadModel(Guid Id, Guid AuthorId, string Title, string Description, string Content);
