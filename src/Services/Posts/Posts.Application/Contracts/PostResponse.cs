using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Posts.Application.Contracts;

[XmlRoot("Post")]
public class PostResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AuthorSummaryResponse? Author { get; set; }
    
    public PostResponse() {}

    public PostResponse(Guid id, string title, string description, string content, AuthorSummaryResponse? author)
    {
        Id = id;
        Title = title;
        Description = description;
        Content = content;
        Author = author;
    }
}