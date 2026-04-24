using System.Xml.Serialization;

namespace Posts.Application.Contracts;

[XmlRoot("AuthorSummary")]
public class AuthorSummaryResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;

    public AuthorSummaryResponse() {}

    public AuthorSummaryResponse(Guid id, string name, string surname)
    {
        Id = id;
        Name = name;
        Surname = surname;
    }
}
