using System.Net;
using System.Net.Http.Json;
using Authors.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Authors.Api.Tests;

public sealed class AuthorsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public AuthorsApiTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetByIdReturnsSeededAuthor()
    {
        var author = await client.GetFromJsonAsync<AuthorResponseContract>($"/authors/{AuthorSeedData.AdaId}");

        Assert.NotNull(author);
        Assert.Equal("Ada", author.Name);
        Assert.Equal("Lovelace", author.Surname);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundForUnknownAuthor()
    {
        var response = await client.GetAsync($"/authors/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record AuthorResponseContract(Guid Id, string Name, string Surname);
}