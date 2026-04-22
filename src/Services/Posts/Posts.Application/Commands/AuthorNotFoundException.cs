namespace Posts.Application.Commands;

public sealed class AuthorNotFoundException : Exception
{
    public AuthorNotFoundException(Guid authorId)
        : base($"Author '{authorId}' was not found.")
    {
    }
}
