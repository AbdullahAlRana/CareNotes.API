using CareNotes.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CareNotes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddScoped<AuthService>()
            .AddScoped<UserService>()
            .AddScoped<NoteService>()
            .AddScoped<PostService>();
}
