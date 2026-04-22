using Authors.Application.Ports;
using Authors.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Authors.Infrastructure.Persistence;

public sealed class SqlAuthorRepository : IAuthorRepository
{
    private readonly SqlAuthorsDbOptions options;

    public SqlAuthorRepository(IOptions<SqlAuthorsDbOptions> options)
    {
        this.options = options.Value;
    }

    public async Task<IReadOnlyCollection<Author>> GetAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT id, name, surname
            FROM authors
            ORDER BY surname, name
            """, connection);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var authors = new List<Author>();

        while (await reader.ReadAsync(cancellationToken))
        {
            authors.Add(MapAuthor(reader));
        }

        return authors;
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            SELECT id, name, surname
            FROM authors
            WHERE id = @id
            """, connection);
        command.Parameters.AddWithValue("id", id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapAuthor(reader) : null;
    }

    public async Task AddAsync(Author author, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            INSERT INTO authors (id, name, surname)
            VALUES (@id, @name, @surname)
            """, connection);
        command.Parameters.AddWithValue("id", author.Id);
        command.Parameters.AddWithValue("name", author.Name);
        command.Parameters.AddWithValue("surname", author.Surname);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Author author, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            UPDATE authors
            SET name = @name,
                surname = @surname
            WHERE id = @id
            """, connection);
        command.Parameters.AddWithValue("id", author.Id);
        command.Parameters.AddWithValue("name", author.Name);
        command.Parameters.AddWithValue("surname", author.Surname);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(options.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand("""
            DELETE FROM authors
            WHERE id = @id
            """, connection);
        command.Parameters.AddWithValue("id", id);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);
        return rows > 0;
    }


    private static Author MapAuthor(SqlDataReader reader)
    {
        return new Author(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2));
    }
}












