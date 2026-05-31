namespace PlantMonitoringAPI.DTOs
{
    public class SettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Description { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateSettingDto
    {
        public string? Value { get; set; }
    }

    public class UpdateSettingsBulkDto
    {
        // Used by settings page to submit a whole form in one request
        public Dictionary<string, string?> Settings { get; set; } = new();
    }
}