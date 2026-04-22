using Authors.Application.Ports;
using Authors.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authors.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AuthorsDb")
            ?? throw new InvalidOperationException("Connection string 'AuthorsDb' is required.");

        services.Configure<SqlAuthorsDbOptions>(options => options.ConnectionString = connectionString);
        services.AddScoped<IAuthorRepository, SqlAuthorRepository>();
        return services;
    }
}
