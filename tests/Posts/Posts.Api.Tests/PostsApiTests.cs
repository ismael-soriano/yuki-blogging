using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Posts.Api.Contracts;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Xunit;

namespace Posts.Api.Tests;

public sealed class PostsApiTests
{
    [Fact]
    public async Task SwaggerJsonIsExposed()
    {
        await using var factory = new WebApplicationFactory<Program>();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PostCreatesResourceAndGetReturnsAuthorWhenRequested()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IAuthorDirectory>(new StubAuthorDirectory());
                });
            });

        var client = factory.CreateClient();
        var authorId = Guid.Parse("9f9df8ca-4314-4d0d-a629-fcb0cead5dae");

        var createResponse = await client.PostAsJsonAsync(
            "/post",
            new CreatePostHttpRequest(authorId, "Title", "Description", "Content"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdPost = await createResponse.Content.ReadFromJsonAsync<PostContract>();
        Assert.NotNull(createdPost);

        var fetchedPost = await client.GetFromJsonAsync<PostContract>($"/post/{createdPost.Id}?includeAuthor=true");

        Assert.NotNull(fetchedPost);
        Assert.NotNull(fetchedPost.Author);
        Assert.Equal("Ada", fetchedPost.Author.Name);
    }

    [Fact]
    public async Task PostReturnsValidationProblemWhenAuthorDoesNotExist()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IAuthorDirectory>(new StubAuthorDirectory(exists: false));
                });
            });

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/post",
            new CreatePostHttpRequest(Guid.NewGuid(), "Title", "Description", "Content"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed class StubAuthorDirectory : IAuthorDirectory
    {
        private readonly bool exists;

        public StubAuthorDirectory(bool exists = true)
        {
            this.exists = exists;
        }

        public Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken) => Task.FromResult(exists);

        public Task<AuthorSummaryResponse?> GetByIdAsync(Guid authorId, CancellationToken cancellationToken)
        {
            var author = exists ? new AuthorSummaryResponse(authorId, "Ada", "Lovelace") : null;
            return Task.FromResult(author);
        }
    }

    private sealed record PostContract(
        Guid Id,
        Guid AuthorId,
        string Title,
        string Description,
        string Content,
        AuthorSummaryResponse? Author);
}
