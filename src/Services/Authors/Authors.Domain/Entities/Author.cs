namespace Authors.Domain.Entities;

public sealed class Author
{
    public Author(Guid id, string name, string surname)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Author id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Author name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(surname))
        {
            throw new ArgumentException("Author surname is required.", nameof(surname));
        }

        Id = id;
        Name = name.Trim();
        Surname = surname.Trim();
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Surname { get; }
}
