using Authors.Application.Ports;
using Authors.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Authors.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorsInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorRepository, InMemoryAuthorRepository>();
        return services;
    }
}
