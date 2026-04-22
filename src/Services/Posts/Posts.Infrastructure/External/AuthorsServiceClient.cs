using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Posts.Application.Contracts;
using Posts.Application.Ports;
using Posts.Infrastructure.Configuration;

namespace Posts.Infrastructure.External;

public sealed class AuthorsServiceClient : IAuthorDirectory
{
    private readonly HttpClient httpClient;

    public AuthorsServiceClient(HttpClient httpClient, IOptions<AuthorsServiceOptions> options)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<bool> AuthorExistsAsync(Guid authorId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/authors/{authorId}", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,
            _ => throw new HttpRequestException($"Author lookup failed with status code {(int)response.StatusCode}.")
        };
    }

    public async Task<AuthorSummaryResponse?> GetByIdAsync(Guid authorId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"/authors/{authorId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthorSummaryResponse>(cancellationToken: cancellationToken);
    }
}
