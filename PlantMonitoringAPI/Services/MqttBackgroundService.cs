using MQTTnet;
using MQTTnet.Client;
using PlantMonitoringAPI.Data;
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

        // maps to the Value property on TelemetryPayload
        // Created once and reused to avoid allocating on every message
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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

            // Fires when the connection to EMQX drops
            // Waits 5 seconds then attempts to reconnect and resubscribe            
            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("Disconnected from MQTT broker. Reconnecting in 5 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                try
                {
                    await _mqttClient.ConnectAsync(options, stoppingToken);
                    await _mqttClient.SubscribeAsync("devices/+/telemetry", cancellationToken: stoppingToken);
                    _logger.LogInformation("Reconnected and resubscribed.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Reconnect cancelled, application shutting down.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Reconnect failed.");
                }
            };

            // Handler called sequentially by default, this could be a problem later but for now it's ok
            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

            await _mqttClient.ConnectAsync(options, stoppingToken);

            // Wildcard + matches exactly one topic segment, devices/1/telemetry
            await _mqttClient.SubscribeAsync("devices/+/telemetry", cancellationToken: stoppingToken);

            _logger.LogInformation("MQTT background service connected and subscribed.");

            // Keep the background service alive until the app shuts down
            // The cancellation token is signalled on app shutdown,
            // which causes this delay to throw OperationCanceledException
            // and the service exits cleanly
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task HandleMessageAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var parts = e.ApplicationMessage.Topic.Split('/');

            if (parts.Length != 3)
            {
                _logger.LogWarning("Unexpected topic format: {Topic}", e.ApplicationMessage.Topic);
                return;
            }

            if (!int.TryParse(parts[1], out var deviceId))
            {
                _logger.LogWarning("Could not parse device ID from topic: {Topic}", e.ApplicationMessage.Topic);
                return;
            }

            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            _logger.LogInformation("Telemetry from device {DeviceId}: {Payload}", deviceId, payload);

            try
            {             
                var data = JsonSerializer.Deserialize<TelemetryPayload>(payload, _jsonOptions);
                if (data == null)
                {
                    _logger.LogWarning("Failed to deserialize telemetry payload from device {DeviceId}: {Payload}", deviceId, payload);
                    return;
                }

                // AppDbContext is scoped and cannot be injected directly into a singleton
                // A new scope is created per message so each DB operation gets its own context instance that is disposed after the save
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var device = await context.Devices.FindAsync(deviceId);
                if (device == null)
                {
                    _logger.LogWarning("Telemetry received for unknown device {DeviceId}", deviceId);
                    return;
                }

                if (device.CurrentPlantId == null)
                {
                    _logger.LogWarning("Device {DeviceId} has no plant assigned — reading discarded", deviceId);
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


        // Called from anywhere to push a command to a device, returns true if the broker accepted the message, false otherwise
        public async Task<bool> SendCommandAsync(int deviceId, object command)
        {
            if (_mqttClient == null || !_mqttClient.IsConnected)
            {
                _logger.LogWarning("Cannot send command to device {DeviceId} — MQTT client not connected", deviceId);
                return false;
            }

            try
            {
                var payload = JsonSerializer.Serialize(command);
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"devices/{deviceId}/commands")
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                await _mqttClient.PublishAsync(message);
                _logger.LogInformation("Sent command to device {DeviceId}: {Payload}", deviceId, payload);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command to device {DeviceId}", deviceId);
                return false;
            }
        }
    }

    public class TelemetryPayload
    {
        public double Value { get; set; }
    }
}