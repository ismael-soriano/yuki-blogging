using System.Text.Json.Serialization;

namespace Posts.Application.Contracts;

public sealed record PostResponse(
    Guid Id,
    Guid AuthorId,
    string Title,
    string Description,
    string Content,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] AuthorSummaryResponse? Author);
