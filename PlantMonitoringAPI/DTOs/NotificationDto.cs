namespace PlantMonitoringAPI.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int? PlantId { get; set; }
        public string? PlantName { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public bool SentEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}