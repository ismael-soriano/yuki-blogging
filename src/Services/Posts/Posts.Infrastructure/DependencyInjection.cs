using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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

        // Configure MongoDB
        services.Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.SectionName));

        // Register MongoDB client and database
        services.AddSingleton<IMongoClient>(sp =>
        {
            var options = configuration.GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>()
                ?? new MongoDbOptions();
            return new MongoClient(options.ConnectionString);
        });

        services.AddScoped(sp =>
        {
            var options = sp.GetRequiredService<IConfiguration>()
                .GetSection(MongoDbOptions.SectionName).Get<MongoDbOptions>()
                ?? new MongoDbOptions();
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(options.DatabaseName);
        });

        // Use MongoDB implementations
        services.AddScoped<IPostEventStore, MongoPostEventStore>();
        services.AddScoped<IPostReadRepository, MongoPostReadRepository>();

        services.AddHttpClient<IAuthorDirectory, AuthorsServiceClient>();
        return services;
    }
}
