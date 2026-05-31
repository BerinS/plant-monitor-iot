using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications
        // Optional query params: ?unreadOnly=true&plantId=1
        // Examples:
        //   api/notifications                    — all notifications
        //   api/notifications?unreadOnly=true    — unread only
        //   api/notifications?plantId=1          — all for plant 1
        //   api/notifications?plantId=1&unreadOnly=true — unread for plant 1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            [FromQuery] int? plantId = null)
        {
            var query = _context.Notifications.AsQueryable();

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            if (plantId.HasValue)
                query = query.Where(n => n.PlantId == plantId.Value);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    PlantId = n.PlantId,
                    PlantName = n.PlantName,
                    Severity = n.Severity,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    SentEmail = n.SentEmail,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Where(n => n.Id == id)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    PlantId = n.PlantId,
                    PlantName = n.PlantName,
                    Severity = n.Severity,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    SentEmail = n.SentEmail,
                    CreatedAt = n.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (notification == null)
                return NotFound(new { message = $"Notification with ID {id} not found." });

            return Ok(notification);
        }

        // PATCH: api/notifications/{id}/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound(new { message = $"Notification with ID {id} not found." });
            
            if (notification.IsRead)
                return Ok(new { message = "Notification was already marked as read." });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read." });
        }

        // PATCH: api/notifications/read-all
        // Optional query param: ?plantId=1 to mark only a specific plant's notifications
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int? plantId = null)
        {
            var query = _context.Notifications
                .Where(n => !n.IsRead);

            if (plantId.HasValue)
                query = query.Where(n => n.PlantId == plantId.Value);

            var unread = await query.ToListAsync();

            if (!unread.Any())
                return Ok(new { message = "No unread notifications found.", updated = 0 });

            foreach (var notification in unread)
                notification.IsRead = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Marked {unread.Count} notification(s) as read.", updated = unread.Count });
        }

        // DELETE: api/notifications/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return NotFound(new { message = $"Notification with ID {id} not found." });

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}