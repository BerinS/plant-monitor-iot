using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Models;
using System.Text;
using System.Text.Json;

namespace PlantMonitoringAPI.Services
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly INotificationService _notificationService;
        private IMqttClient? _mqttClient;

        // Used for debounce check and notification creation
        private const string DRY_NOTIFICATION_TITLE = "Plant is dry";

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MqttBackgroundService(
            ILogger<MqttBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            INotificationService notificationService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _config = config;
            _notificationService = notificationService;
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

            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageAsync;

            await _mqttClient.ConnectAsync(options, stoppingToken);
            await _mqttClient.SubscribeAsync("devices/+/telemetry", cancellationToken: stoppingToken);

            _logger.LogInformation("MQTT background service connected and subscribed.");

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

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Plant included so MoistureThreshold is available for the ryness check 
                var device = await context.Devices
                    .Include(d => d.Plant)
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

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

                var dataPoint = new SensorData
                {
                    PlantId = device.CurrentPlantId.Value,
                    MoistureValue = data.Value,
                    MeasuredAt = DateTime.UtcNow
                };

                context.SensorData.Add(dataPoint);
                await context.SaveChangesAsync();

                _logger.LogInformation("Saved reading {Value}% for plant {PlantId}", data.Value, device.CurrentPlantId);

                // Dryness check runs after the reading is saved, plant is already loaded via Include 
                if (device.Plant != null)
                    await CheckDrynessAsync(device.Plant, data.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process telemetry from device {DeviceId}", deviceId);
            }
        }

        private async Task CheckDrynessAsync(Plant plant, double moisture)
        {
            try
            {
                // threshold ccheck                
                if (plant.MoistureThreshold == null)
                    return;

                // in-memory comparison
                if (moisture >= plant.MoistureThreshold.Value)
                    return;

                // debounce prevents a new notification on every reading
                // When notification is read, the next dry reading will create a notification
                var alreadyNotified = await _notificationService
                    .UnreadExistsAsync(plant.Id, DRY_NOTIFICATION_TITLE);

                if (alreadyNotified)
                {
                    _logger.LogInformation(
                        "Plant {PlantId} is dry but unread notification already exists — skipping",
                        plant.Id);
                    return;
                }

                var message =
                    $"{plant.Name} moisture is at {moisture:F0}% — " +
                    $"below the threshold of {plant.MoistureThreshold.Value}%.";

                await _notificationService.CreateAsync(
                    title: DRY_NOTIFICATION_TITLE,
                    message: message,
                    severity: NotificationSeverity.Warning,
                    plantId: plant.Id,
                    plantName: plant.Name);

                _logger.LogInformation(
                    "Dry notification created for plant {PlantId} at {Moisture}%",
                    plant.Id, moisture);
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, "Dryness check failed for plant {PlantId}", plant.Id);
            }
        }

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