using CareNotes.Application.Abstractions;
using CareNotes.Infrastructure.Persistence;
using CareNotes.Infrastructure.Persistence.Repositories;
using CareNotes.Infrastructure.Persistence.Schemas;
using CareNotes.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareNotes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoSettings>(configuration.GetSection("Mongo"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddSingleton<UserSchema>();
        services.AddSingleton<NoteSchema>();
        services.AddSingleton<PostSchema>();
        services.AddSingleton<MongoContext>();
        services.AddHostedService<MongoInitializer>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        return services;
    }
}
