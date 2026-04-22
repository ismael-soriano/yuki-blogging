using System.Net;
using System.Net.Http.Json;
using Authors.Api.Contracts;
using Authors.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace Authors.Api.IntegrationTests;

public sealed class AuthorsControllerIntegrationTests : IAsyncLifetime
{
    private MsSqlContainer? container;

    public async Task InitializeAsync()
    {
        container = new MsSqlBuilder()
            .WithPassword("TestPassword123!")
            .Build();

        await container.StartAsync();
        await InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (container is not null)
        {
            await container.StopAsync();
        }
    }

    [Fact]
    public async Task GetAllReturnsAuthorsFromDatabase()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authors = await response.Content.ReadFromJsonAsync<List<AuthorResponseContract>>();
        Assert.NotNull(authors);
        Assert.NotEmpty(authors);
    }

    [Fact]
    public async Task PostCreatesAuthorInDatabaseAndGetByIdReturnsIt()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/authors", new UpsertAuthorHttpRequest("Marie", "Curie"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(created);

        var fetched = await client.GetFromJsonAsync<AuthorResponseContract>($"/authors/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal("Marie", fetched.Name);
        Assert.Equal("Curie", fetched.Surname);
    }

    [Fact]
    public async Task PutUpdatesAuthorInDatabase()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        // Create
        var createResponse = await client.PostAsJsonAsync("/authors", new UpsertAuthorHttpRequest("Nikola", "Tesla"));
        var created = await createResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(created);

        // Update
        var updateResponse = await client.PutAsJsonAsync($"/authors/{created.Id}", new UpsertAuthorHttpRequest("Nikola", "Tesla-Updated"));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(updated);
        Assert.Equal("Tesla-Updated", updated.Surname);

        // Verify persistence
        var fetched = await client.GetFromJsonAsync<AuthorResponseContract>($"/authors/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal("Tesla-Updated", fetched.Surname);
    }

    [Fact]
    public async Task DeleteRemovesAuthorFromDatabase()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        // Create
        var createResponse = await client.PostAsJsonAsync("/authors", new UpsertAuthorHttpRequest("Richard", "Feynman"));
        var created = await createResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(created);

        // Delete
        var deleteResponse = await client.DeleteAsync($"/authors/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify deletion
        var getResponse = await client.GetAsync($"/authors/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var connectionString = new SqlConnectionStringBuilder(container!.GetConnectionString())
                    {
                        InitialCatalog = "authors"
                    }.ConnectionString;
                    services.PostConfigure<SqlAuthorsDbOptions>(options => options.ConnectionString = connectionString);
                });
            });
    }

    private async Task InitializeDatabaseAsync()
    {
        var connectionStringBuilder = new SqlConnectionStringBuilder(container!.GetConnectionString())
        {
            InitialCatalog = "master"
        };

        await using (var masterConnection = new SqlConnection(connectionStringBuilder.ConnectionString))
        {
            await masterConnection.OpenAsync();
            await using var createDbCommand = new SqlCommand("""
                IF DB_ID(N'authors') IS NULL
                BEGIN
                    CREATE DATABASE [authors];
                END
                """, masterConnection);
            await createDbCommand.ExecuteNonQueryAsync();
        }

        var authorsConnectionString = new SqlConnectionStringBuilder(container.GetConnectionString())
        {
            InitialCatalog = "authors"
        }.ConnectionString;

        await using var authorsConnection = new SqlConnection(authorsConnectionString);
        await authorsConnection.OpenAsync();
        await using var createSchemaAndSeed = new SqlCommand("""
            IF OBJECT_ID(N'dbo.authors', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.authors (
                    id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    name NVARCHAR(255) NOT NULL,
                    surname NVARCHAR(255) NOT NULL
                );
            END

            IF NOT EXISTS (SELECT 1 FROM dbo.authors WHERE id = '9f9df8ca-4314-4d0d-a629-fcb0cead5dae')
            BEGIN
                INSERT INTO dbo.authors (id, name, surname)
                VALUES ('9f9df8ca-4314-4d0d-a629-fcb0cead5dae', 'Ada', 'Lovelace');
            END

            IF NOT EXISTS (SELECT 1 FROM dbo.authors WHERE id = '78e270c0-1134-4f52-9d90-dc559b1cbec5')
            BEGIN
                INSERT INTO dbo.authors (id, name, surname)
                VALUES ('78e270c0-1134-4f52-9d90-dc559b1cbec5', 'Linus', 'Torvalds');
            END
            """, authorsConnection);
        await createSchemaAndSeed.ExecuteNonQueryAsync();
    }

    private sealed record AuthorResponseContract(Guid Id, string Name, string Surname);
}

