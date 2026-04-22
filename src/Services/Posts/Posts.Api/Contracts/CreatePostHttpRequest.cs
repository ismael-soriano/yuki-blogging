using System.ComponentModel.DataAnnotations;

namespace Posts.Api.Contracts;

public sealed record CreatePostHttpRequest(
    [property: Required] Guid AuthorId,
    [property: Required] string Title,
    [property: Required] string Description,
    [property: Required] string Content);
