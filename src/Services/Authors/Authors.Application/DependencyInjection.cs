using Microsoft.Extensions.DependencyInjection;

namespace Authors.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorsApplication(this IServiceCollection services)
    {
        services.AddScoped<Queries.GetAuthorByIdQueryHandler>();
        return services;
    }
}
