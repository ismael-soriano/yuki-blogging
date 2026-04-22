using Microsoft.Extensions.DependencyInjection;

namespace Posts.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPostsApplication(this IServiceCollection services)
    {
        services.AddScoped<Commands.CreatePostCommandHandler>();
        services.AddScoped<Queries.GetPostByIdQueryHandler>();
        return services;
    }
}
