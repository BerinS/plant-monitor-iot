using MQTTnet;
using MQTTnet.Client;
using PlantMonitoringAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace PlantMonitoringAPI.Services
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private IMqttClient? _mqttClient;

        public MqttBackgroundService(
            ILogger<MqttBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration config)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    _config["Mqtt:BrokerHost"],
                    int.Parse(_config["Mqtt:BrokerPort"] ?? "1883"))
                .WithCredentials(
                    _config["Mqtt:Username"],
                    _config["Mqtt:Password"])
                .WithClientId("backend-service")
                .WithCleanSession()
                .Build();

            // Reconnect automatically if broker restarts
            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("Disconnected from MQTT broker. Reconnecting...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                try { await _mqttClient.ConnectAsync(options, stoppingToken); }
                catch { _logger.LogError("Reconnect failed."); }
            };

            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

            await _mqttClient.ConnectAsync(options, stoppingToken);

            // Subscribe to telemetry from all devices using wildcard
            await _mqttClient.SubscribeAsync("devices/+/telemetry", cancellationToken: stoppingToken);

            _logger.LogInformation("MQTT background service connected and subscribed.");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            // Topic format: devices/{deviceId}/telemetry
            var parts = e.ApplicationMessage.Topic.Split('/');
            if (parts.Length != 3) return;

            if (!int.TryParse(parts[1], out var deviceId)) return;

            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            _logger.LogInformation("Telemetry from device {DeviceId}: {Payload}", deviceId, payload);

            try
            {
                var data = JsonSerializer.Deserialize<TelemetryPayload>(payload);
                if (data == null) return;

                // EF Core DbContext is scoped, not singleton
                // creating a scope here to resolve it correctly inside this singleton background service
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var device = await context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    _logger.LogWarning("Received telemetry for unknown device {DeviceId}", deviceId);
                    return;
                }

                if (device.CurrentPlantId == null)
                {
                    _logger.LogWarning("Device {DeviceId} has no plant assigned", deviceId);
                    return;
                }

                var dataPoint = new Models.SensorData
                {
                    PlantId = device.CurrentPlantId.Value,
                    MoistureValue = data.Value,
                    MeasuredAt = DateTime.UtcNow
                };

                context.SensorData.Add(dataPoint);
                await context.SaveChangesAsync();

                _logger.LogInformation("Saved reading {Value}% for plant {PlantId}", data.Value, device.CurrentPlantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process telemetry from device {DeviceId}", deviceId);
            }
        }

        // For sending commands to a device
        public async Task SendCommandAsync(int deviceId, object command)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected) return;

            var payload = JsonSerializer.Serialize(command);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"devices/{deviceId}/commands")
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(message);
            _logger.LogInformation("Sent command to device {DeviceId}: {Payload}", deviceId, payload);
        }
    }

    public class TelemetryPayload
    {
        public double Value { get; set; }
    }
}