namespace Posts.Infrastructure.Configuration;

public sealed class AuthorsServiceOptions
{
    public const string SectionName = "Services:Authors";

    public string BaseUrl { get; init; } = "http://localhost:8081";
}
