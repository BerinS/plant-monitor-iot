using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantMonitoringAPI.Models
{
    [Table("event_log")]
    public class EventLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("event_type")]
        public string EventType { get; set; } = string.Empty;

        // history stays when plant is deleted
        [Column("plant_id")]
        public int? PlantId { get; set; }

        // plant name at time of event
        [MaxLength(100)]
        [Column("plant_name")]
        public string? PlantName { get; set; }

        // history stays when device is deleted
        [Column("device_id")]
        public int? DeviceId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("triggered_by")]
        public string TriggeredBy { get; set; } = "system";

        // moisture reading at moment of event
        [Column("moisture_at_time")]
        public double? MoistureAtTime { get; set; }

        // for watering events
        [Column("duration_seconds")]
        public int? DurationSeconds { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // event type constants
    public static class EventType
    {
        public const string WateringManual = "watering_manual";
        public const string WateringAuto = "watering_auto";
        public const string SensorOffline = "sensor_offline";
        public const string SensorOnline = "sensor_online";
    }

    public static class TriggeredBy
    {
        public const string Manual = "manual";  // user action via frontend
        public const string Auto = "auto";    // threshold check fired
        public const string System = "system";  // internal backend logic
    }
}