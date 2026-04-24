using System.Xml.Serialization;

namespace Authors.Application.Contracts;

[XmlRoot("Author")]
public class AuthorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    
    public AuthorResponse() {}

    public AuthorResponse(Guid id, string name, string surname)
    {
        Id = id;
        Name = name;
        Surname = surname;
    }
}