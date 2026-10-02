using CareNotes.Application.Abstractions;
using CareNotes.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CareNotes.Infrastructure.Persistence;

public class MongoInitializer(IServiceProvider services, IConfiguration configuration) : IHostedService
{
    /// Creates indexes and seeds the initial admin account if configured.
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<MongoContext>().EnsureIndexesAsync(ct);

        var email = configuration["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            return;

        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        if (await users.GetByEmailAsync(email, ct) is not null)
            return;

        await users.CreateAsync(new User
        {
            Name = configuration["Seed:AdminName"] ?? "Administrator",
            Email = email,
            PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher>().Hash(password),
            Role = Roles.Admin
        }, ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
