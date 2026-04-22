namespace Authors.Application.Commands;

public sealed record UpdateAuthorCommand(Guid AuthorId, string Name, string Surname);

