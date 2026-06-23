using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlantMonitoringAPI.Services;

namespace PlantMonitoringAPI.Tests;

// Test double that replaces the real MqttBackgroundService.
// Records command-publish calls instead of contacting a broker, and lets
// individual tests configure whether SendCommandAsync should "succeed".
public class FakeMqttBackgroundService : MqttBackgroundService
{
    public List<SentCommand> SentCommands { get; } = new();
    public bool SendCommandShouldSucceed { get; set; } = true;

    public FakeMqttBackgroundService(
        ILogger<MqttBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        INotificationService notificationService)
        : base(logger, scopeFactory, config, notificationService)
    {
    }

    // No-op: the real implementation connects to EMQX; tests skip that.
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public override Task<bool> SendCommandAsync(int deviceId, object command)
    {
        SentCommands.Add(new SentCommand(deviceId, command));
        return Task.FromResult(SendCommandShouldSucceed);
    }

    public record SentCommand(int DeviceId, object Command);
}
