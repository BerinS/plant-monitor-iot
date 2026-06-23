using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Services;

namespace PlantMonitoringAPI.Tests;

// Spins up the application in-memory with three substitutions:
//   1. AppDbContext is rebound to a SQLite in-memory connection (no Docker, no PostgreSQL).
//   2. All IHostedService registrations are dropped so the MQTT and Email loops don't run.
//   3. MqttBackgroundService is replaced by FakeMqttBackgroundService so the watering controller
//      can publish commands without touching a broker.
public class PlantMonitoringWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public PlantMonitoringWebApplicationFactory()
    {
        // ":memory:" databases live for the lifetime of the connection.
        // Keeping the connection open at the factory level means every scoped
        // DbContext created during tests sees the same database.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // EF Core refuses to register two database providers in the same container.
            // Strip every EF Core / Npgsql service that Program.cs registered, then add SQLite cleanly.
            //
            // NOTE: this blanket removal is safe because the application has exactly one DbContext
            // (AppDbContext). If a second DbContext is ever added, this filter will over-remove and
            // the test setup will need to target AppDbContext-specific registrations only.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    (d.ServiceType.FullName?.StartsWith("Microsoft.EntityFrameworkCore") ?? false) ||
                    (d.ServiceType.FullName?.StartsWith("Npgsql") ?? false))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // Drop every hosted service so background loops do not run during tests.
            var hostedDescriptors = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var d in hostedDescriptors)
                services.Remove(d);

            // Replace the singleton MqttBackgroundService with the test double.
            RemoveDescriptor<MqttBackgroundService>(services);
            services.AddSingleton<MqttBackgroundService>(sp => new FakeMqttBackgroundService(
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MqttBackgroundService>>(),
                sp.GetRequiredService<IServiceScopeFactory>(),
                sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>(),
                sp.GetRequiredService<INotificationService>()));

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public FakeMqttBackgroundService GetMqttFake() =>
        (FakeMqttBackgroundService)Services.GetRequiredService<MqttBackgroundService>();

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }
        base.Dispose(disposing);
    }

    private static void RemoveDescriptor<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
            services.Remove(descriptor);
    }
}
