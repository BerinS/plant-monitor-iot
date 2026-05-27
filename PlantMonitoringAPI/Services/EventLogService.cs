using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Services
{
    public interface IEventLogService
    {
        // Logs event to the event_log table, returns false if write fails
        Task<bool> LogAsync(
            string eventType,
            string triggeredBy,
            int? plantId,
            string? plantName,
            int? deviceId,
            double? moistureAtTime,
            int? durationSeconds,
            string? notes);
    }

    public class EventLogService : IEventLogService
    {
        // Singleton so cannot inject AppDbContext directly
        // IServiceScopeFactory creates fresh scope and DbContext per log write
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<EventLogService> _logger;
        
        public EventLogService(
            IServiceScopeFactory scopeFactory,
            ILogger<EventLogService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<bool> LogAsync(
            string eventType,
            string triggeredBy,
            int? plantId = null,
            string? plantName = null,
            int? deviceId = null,
            double? moistureAtTime = null,
            int? durationSeconds = null,
            string? notes = null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var entry = new EventLog
                {
                    EventType = eventType,
                    TriggeredBy = triggeredBy,
                    PlantId = plantId,
                    PlantName = plantName,
                    DeviceId = deviceId,
                    MoistureAtTime = moistureAtTime,
                    DurationSeconds = durationSeconds,
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
                };

                context.EventLog.Add(entry);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Event logged — type: {EventType}, triggeredBy: {TriggeredBy}, plant: {PlantName}, device: {DeviceId}",
                    eventType, triggeredBy, plantName ?? "none", deviceId?.ToString() ?? "none");

                return true;
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex,
                    "Failed to log event — type: {EventType}, plantId: {PlantId}, deviceId: {DeviceId}",
                    eventType, plantId, deviceId);

                return false;
            }
        }
    }
}