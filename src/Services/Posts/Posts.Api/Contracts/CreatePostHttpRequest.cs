using System.ComponentModel.DataAnnotations;

namespace Posts.Api.Contracts;

public sealed record CreatePostHttpRequest(
    [param: Required] Guid AuthorId,
    [param: Required] string Title,
    [param: Required] string Description,
    [param: Required] string Content);
