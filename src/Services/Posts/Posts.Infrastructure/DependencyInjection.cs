using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Posts.Application.Ports;
using Posts.Infrastructure.Configuration;
using Posts.Infrastructure.External;
using Posts.Infrastructure.Persistence;

namespace Posts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthorsServiceOptions>(configuration.GetSection(AuthorsServiceOptions.SectionName));
        services.AddSingleton<IPostEventStore, InMemoryPostEventStore>();
        services.AddSingleton<IPostReadRepository, InMemoryPostReadRepository>();
        services.AddHttpClient<IAuthorDirectory, AuthorsServiceClient>();
        return services;
    }
}
