using System.Net;
using System.Net.Http.Json;
using Authors.Api.Contracts;
using Authors.Application.Ports;
using Authors.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Authors.Api.UnitTests;

public sealed class AuthorsControllerUnitTests
{
    [Fact]
    public async Task PostCreatesAuthorAndGetByIdReturnsIt()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/authors", new UpsertAuthorHttpRequest("Grace", "Hopper"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(created);

        var fetched = await client.GetFromJsonAsync<AuthorResponseContract>($"/authors/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal("Grace", fetched.Name);
        Assert.Equal("Hopper", fetched.Surname);
    }

    [Fact]
    public async Task PutUpdatesExistingAuthor()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/authors", new UpsertAuthorHttpRequest("Alan", "Turing"));
        var created = await createResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync($"/authors/{created.Id}", new UpsertAuthorHttpRequest("Alonzo", "Church"));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(updated);
        Assert.Equal("Alonzo", updated.Name);
        Assert.Equal("Church", updated.Surname);
    }

    [Fact]
    public async Task DeleteRemovesAuthor()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/authors", new UpsertAuthorHttpRequest("Donald", "Knuth"));
        var created = await createResponse.Content.ReadFromJsonAsync<AuthorResponseContract>();
        Assert.NotNull(created);

        var deleteResponse = await client.DeleteAsync($"/authors/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/authors/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForUnknownAuthor()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/authors/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SwaggerJsonIsExposed()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    private static WebApplicationFactory<Program> CreateFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IAuthorRepository>();
                    services.AddSingleton<IAuthorRepository>(new StubAuthorRepository());
                });
            });
    }

    private sealed class StubAuthorRepository : IAuthorRepository
    {
        private readonly Dictionary<Guid, Author> authors = new()
        {
            [Guid.Parse("9f9df8ca-4314-4d0d-a629-fcb0cead5dae")] = new(Guid.Parse("9f9df8ca-4314-4d0d-a629-fcb0cead5dae"), "Ada", "Lovelace")
        };

        public Task<IReadOnlyCollection<Author>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Author>>(authors.Values.ToArray());
        }

        public Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            authors.TryGetValue(id, out var author);
            return Task.FromResult(author);
        }

        public Task AddAsync(Author author, CancellationToken cancellationToken)
        {
            authors[author.Id] = author;
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Author author, CancellationToken cancellationToken)
        {
            if (!authors.ContainsKey(author.Id))
            {
                return Task.FromResult(false);
            }

            authors[author.Id] = author;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(authors.Remove(id));
        }
    }

    private sealed record AuthorResponseContract(Guid Id, string Name, string Surname);
}

