using Microsoft.Extensions.DependencyInjection;

namespace Posts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsApplication(this IServiceCollection services)
    {
        services.AddScoped<Commands.CreatePostCommandHandler>();
        services.AddScoped<Commands.UpdatePostCommandHandler>();
        services.AddScoped<Commands.DeletePostCommandHandler>();
        services.AddScoped<Queries.GetPostByIdQueryHandler>();
        services.AddScoped<Queries.GetAllPostsQueryHandler>();
        return services;
    }
}
