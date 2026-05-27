using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Services
{
    public interface INotificationService
    {
        Task<bool> CreateAsync(
            string title,
            string message,
            string severity,
            int? plantId,
            string? plantName);

        // Checks whether an unread notification with this title already exists for a plant
        Task<bool> UnreadExistsAsync(int plantId, string title);
    }

    public class NotificationService : INotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationService> _logger;

        // IServiceScopeFactory because NotificationService is a singleton so it outlives any single HTTP request
        // AppDbContext is scoped (one per request), so it cannot be injected directly into a singleton.
        // A scope is created per DB operation and disposed immediately after
        public NotificationService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<bool> CreateAsync(
            string title,
            string message,
            string severity = NotificationSeverity.Warning,
            int? plantId = null,
            string? plantName = null)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var notification = new Notification
                {
                    Title = title,
                    Message = message,
                    Severity = severity,
                    PlantId = plantId,
                    PlantName = plantName,
                    IsRead = false,
                    SentEmail = false,
                    CreatedAt = DateTime.UtcNow
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Notification created — severity: {Severity}, plant: {PlantName}, title: {Title}",
                    severity, plantName ?? "none", title);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create notification — title: {Title}, plantId: {PlantId}",
                    title, plantId);

                return false;
            }
        }

        // Returns true if a matching unread notification exists
        public async Task<bool> UnreadExistsAsync(int plantId, string title)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                return await context.Notifications.AnyAsync(n =>
                    n.PlantId == plantId &&
                    n.Title == title &&
                    !n.IsRead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to check existing notifications for plant {PlantId}",
                    plantId);

                return false;
            }
        }
    }

    public static class NotificationSeverity
    {
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Critical = "critical";
    }
}