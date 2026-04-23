using System.ComponentModel.DataAnnotations;

namespace Posts.Api.Contracts;

public sealed record UpdatePostHttpRequest(
    [param: Required] string Title,
    [param: Required] string Description,
    [param: Required] string Content);

