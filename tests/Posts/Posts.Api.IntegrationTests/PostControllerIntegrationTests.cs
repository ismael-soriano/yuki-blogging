using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using Posts.Api.Contracts;
using Posts.Application;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Testcontainers.MongoDb;
using Xunit;

namespace Posts.Api.IntegrationTests;

public sealed class PostControllerIntegrationTests : IAsyncLifetime
{
    private MongoDbContainer? container;
    private IMongoClient? mongoClient;

    public async Task InitializeAsync()
    {
        container = new MongoDbBuilder()
            .WithCleanUp(true)
            .Build();

        await container.StartAsync();
        mongoClient = new MongoClient(container.GetConnectionString());
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
    public async Task SwaggerJsonIsExposed()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PostCreatesResourceAndGetReturnsAuthorWhenRequested()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        var authorId = Guid.Parse("9f9df8ca-4314-4d0d-a629-fcb0cead5dae");

        var createResponse = await client.PostAsJsonAsync(
            "/post",
            new CreatePostHttpRequest(authorId, "Title", "Description", "Content"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdPost = await createResponse.Content.ReadFromJsonAsync<PostContract>();
        Assert.NotNull(createdPost);

        var getResponse = await client.GetAsync($"/post/{createdPost.Id}?includeAuthor=true");
        var getBody = await getResponse.Content.ReadAsStringAsync();

        // Direct DB check for diagnosis
        var postsTestDb = mongoClient!.GetDatabase("posts_test");
        var col = postsTestDb.GetCollection<MongoDB.Bson.BsonDocument>("posts");
        var allDocs = await col.Find(MongoDB.Bson.BsonDocument.Parse("{}")).ToListAsync();
        var storedDoc = allDocs.FirstOrDefault();
        var storedIdElement = storedDoc?["Id"];

        // Try matching by the exact binary value
        var binaryStandard = new MongoDB.Bson.BsonBinaryData(createdPost.Id, MongoDB.Bson.GuidRepresentation.Standard);
        var matchingDocs = await col.Find(new MongoDB.Bson.BsonDocument("Id", binaryStandard)).ToListAsync();

        Assert.True(getResponse.IsSuccessStatusCode, 
            $"GET failed with {getResponse.StatusCode}. " +
            $"createdPost.Id={createdPost.Id}. " +
            $"Stored Id: type={storedIdElement?.BsonType}, val={storedIdElement}, subtype={(storedIdElement as MongoDB.Bson.BsonBinaryData)?.SubType}. " +
            $"Filter binary subtype={binaryStandard.SubType}. " +
            $"Direct filter match count={matchingDocs.Count}. " +
            $"Body: {getBody}");

        var fetchedPost = await getResponse.Content.ReadFromJsonAsync<PostContract>();

        Assert.NotNull(fetchedPost);
        Assert.NotNull(fetchedPost.Author);
        Assert.Equal("Ada", fetchedPost.Author.Name);
    }

    [Fact]
    public async Task PostReturnsValidationProblemWhenAuthorDoesNotExist()
    {
        await using var factory = CreateFactory(authorExists: false);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/post",
            new CreatePostHttpRequest(Guid.NewGuid(), "Title", "Description", "Content"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByIdReturnsNotFoundWhenPostDoesNotExist()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/post/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutUpdatesPostInDatabase()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        var authorId = Guid.Parse("9f9df8ca-4314-4d0d-a629-fcb0cead5dae");

        var createResponse = await client.PostAsJsonAsync(
            "/post",
            new CreatePostHttpRequest(authorId, "Original Title", "Original Description", "Original Content"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<PostContract>();
        Assert.NotNull(created);

        var updateResponse = await client.PutAsJsonAsync(
            $"/post/{created.Id}",
            new UpdatePostHttpRequest("Updated Title", "Updated Description", "Updated Content"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var fetched = await client.GetFromJsonAsync<PostContract>($"/post/{created.Id}");
        Assert.NotNull(fetched);
        Assert.Equal("Updated Title", fetched.Title);
        Assert.Equal("Updated Description", fetched.Description);
    }

    [Fact]
    public async Task DeleteSoftDeletesPostInDatabase()
    {
        await using var factory = CreateFactory();
        var client = factory.CreateClient();
        var authorId = Guid.Parse("9f9df8ca-4314-4d0d-a629-fcb0cead5dae");

        var createResponse = await client.PostAsJsonAsync(
            "/post",
            new CreatePostHttpRequest(authorId, "Post to Delete", "Desc", "Content"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<PostContract>();
        Assert.NotNull(created);

        var deleteResponse = await client.DeleteAsync($"/post/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/post/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory(bool authorExists = true)
    {
        var connectionString = container!.GetConnectionString();

        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Override configuration so the infrastructure layer picks up the test container
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["MongoDB:ConnectionString"] = connectionString,
                        ["MongoDB:DatabaseName"] = "posts_test",
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    // Remove existing singleton IMongoClient (registered at startup before config override)
                    services.RemoveAll<IMongoClient>();
                    services.AddSingleton<IMongoClient>(_ => mongoClient!);

                    services.AddSingleton<IAuthorDirectory>(new StubAuthorDirectory(authorExists));
                });
            });
    }

    private async Task InitializeDatabaseAsync()
    {
        var db = mongoClient!.GetDatabase("posts_test");

        var names = await (await db.ListCollectionNamesAsync()).ToListAsync();

        if (!names.Contains("posts"))
            await db.CreateCollectionAsync("posts");

        if (!names.Contains("event_streams"))
            await db.CreateCollectionAsync("event_streams");

        var postsCollection = db.GetCollection<MongoDB.Bson.BsonDocument>("posts");
        await postsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                new MongoDB.Bson.BsonDocument("Id", 1),
                new CreateIndexOptions { Unique = true }));

        var eventCollection = db.GetCollection<MongoDB.Bson.BsonDocument>("event_streams");
        await eventCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<MongoDB.Bson.BsonDocument>(
                new MongoDB.Bson.BsonDocument("StreamId", 1),
                new CreateIndexOptions { Unique = true }));
    }

    private sealed class StubAuthorDirectory : IAuthorDirectory
    {
        private readonly bool exists;

        public StubAuthorDirectory(bool exists = true)
        {
            this.exists = exists;
        }

        public Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken) =>
            Task.FromResult(exists);

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
