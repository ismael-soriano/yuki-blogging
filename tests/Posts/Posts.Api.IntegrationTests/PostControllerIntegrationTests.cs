using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Posts.Api.Contracts;
using Posts.Application;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Domain.Abstractions;
using Xunit;

namespace Posts.Api.IntegrationTests;

public sealed class PostControllerIntegrationTests
{
     [Fact]
     public async Task SwaggerJsonIsExposed()
     {
         await using var factory = new WebApplicationFactory<Program>()
             .WithWebHostBuilder(builder =>
             {
                 builder.ConfigureTestServices(services =>
                 {
                     services.AddSingleton<IAuthorDirectory>(new StubAuthorDirectory());
                     services.AddSingleton<IPostEventStore>(new InMemoryPostEventStore());
                     services.AddSingleton<IPostReadRepository>(new InMemoryPostReadRepository());
                 });
             });
         
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
                     services.AddSingleton<IPostEventStore>(new InMemoryPostEventStore());
                     services.AddSingleton<IPostReadRepository>(new InMemoryPostReadRepository());
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
                     services.AddSingleton<IPostEventStore>(new InMemoryPostEventStore());
                     services.AddSingleton<IPostReadRepository>(new InMemoryPostReadRepository());
                 });
             });

         var client = factory.CreateClient();

         var response = await client.PostAsJsonAsync(
             "/post",
             new CreatePostHttpRequest(Guid.NewGuid(), "Title", "Description", "Content"));

         Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
     }

     [Fact]
     public async Task GetByIdReturnsNotFoundWhenPostDoesNotExist()
     {
         await using var factory = new WebApplicationFactory<Program>()
             .WithWebHostBuilder(builder =>
             {
                 builder.ConfigureTestServices(services =>
                 {
                     services.AddSingleton<IAuthorDirectory>(new StubAuthorDirectory());
                     services.AddSingleton<IPostEventStore>(new InMemoryPostEventStore());
                     services.AddSingleton<IPostReadRepository>(new InMemoryPostReadRepository());
                 });
             });

         var client = factory.CreateClient();

         var response = await client.GetAsync($"/post/{Guid.NewGuid()}");

         Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

     private sealed class InMemoryPostEventStore : IPostEventStore
     {
         private readonly Dictionary<Guid, List<IDomainEvent>> streams = new();
         private readonly object sync = new();

         public Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
         {
             lock (sync)
             {
                 if (!streams.TryGetValue(streamId, out var events))
                 {
                     events = [];
                     streams[streamId] = events;
                 }

                 events.AddRange(domainEvents);
             }

             return Task.CompletedTask;
         }
     }

     private sealed class InMemoryPostReadRepository : IPostReadRepository
     {
         private readonly Dictionary<Guid, PostReadModel> posts = new();
         private readonly object sync = new();

         public Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
         {
             lock (sync)
             {
                 posts.TryGetValue(id, out var post);
                 return Task.FromResult(post);
             }
         }

         public Task<IReadOnlyList<PostReadModel>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
         {
             lock (sync)
             {
                 var result = posts.Values
                     .OrderByDescending(p => p.Id)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();
                 return Task.FromResult((IReadOnlyList<PostReadModel>)result);
             }
         }

         public Task SaveAsync(PostReadModel post, CancellationToken cancellationToken)
         {
             lock (sync)
             {
                 posts[post.Id] = post;
             }

             return Task.CompletedTask;
         }

         public Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken)
         {
             lock (sync)
             {
                 posts[post.Id] = post;
             }

             return Task.CompletedTask;
         }

         public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
         {
             lock (sync)
             {
                 if (posts.TryGetValue(id, out var post))
                 {
                     posts[id] = post with { IsDeleted = true };
                 }
             }

             return Task.CompletedTask;
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
