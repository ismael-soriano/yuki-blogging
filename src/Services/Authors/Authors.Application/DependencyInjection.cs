using Microsoft.Extensions.DependencyInjection;

namespace Authors.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorsApplication(this IServiceCollection services)
    {
        services.AddScoped<Commands.CreateAuthorCommandHandler>();
        services.AddScoped<Commands.UpdateAuthorCommandHandler>();
        services.AddScoped<Commands.DeleteAuthorCommandHandler>();
        services.AddScoped<Queries.GetAuthorsQueryHandler>();
        services.AddScoped<Queries.GetAuthorByIdQueryHandler>();
        return services;
    }
}
