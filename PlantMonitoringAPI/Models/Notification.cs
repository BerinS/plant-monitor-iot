using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantMonitoringAPI.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        // no FK constraint, plant can be deleted without losing notification history
        [Column("plant_id")]
        public int? PlantId { get; set; }

        // plant name at time of notification
        [MaxLength(100)]
        [Column("plant_name")]
        public string? PlantName { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("severity")]
        public string Severity { get; set; } = "warning";

        [Required]
        [MaxLength(100)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Required]
        [Column("sent_email")]
        public bool SentEmail { get; set; } = false;

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Severity constants
    public static class NotificationSeverity
    {
        public const string Info = "info";
        public const string Warning = "warning";
        public const string Critical = "critical";
    }
}