using System.ComponentModel;

namespace Posts.Api.Contracts;

public sealed record GetAllPostsHttpRequest
{
    [DefaultValue(1)]
    public int Page { get; init; } = 1;
    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;
    [DefaultValue(false)]
    public bool IncludeAuthor { get; init; } = false;
}

