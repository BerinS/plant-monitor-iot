namespace PlantMonitoringAPI.DTOs
{
    // Read-only projection of an event_log row for the settings page viewer
    public class EventLogDto
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int? PlantId { get; set; }
        public string? PlantName { get; set; }
        public int? DeviceId { get; set; }
        public string TriggeredBy { get; set; } = string.Empty;
        public double? MoistureAtTime { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
