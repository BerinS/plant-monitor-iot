using Microsoft.Extensions.DependencyInjection;
using PlantMonitoringAPI.Data;
using Xunit;

namespace PlantMonitoringAPI.Tests;

// Base class for all integration tests.
// IClassFixture<PlantMonitoringWebApplicationFactory> creates one factory per
// test class (cheap), IAsyncLifetime resets the database before each test
// method so individual tests never see leftover state from siblings.
public abstract class IntegrationTestBase :
    IClassFixture<PlantMonitoringWebApplicationFactory>,
    IAsyncLifetime
{
    protected readonly PlantMonitoringWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(PlantMonitoringWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        Factory.ResetDatabase();
        Factory.GetMqttFake().SentCommands.Clear();
        Factory.GetMqttFake().SendCommandShouldSucceed = true;
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected void WithDb(Action<AppDbContext> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        action(db);
        db.SaveChanges();
    }
}
