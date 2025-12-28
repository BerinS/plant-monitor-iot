namespace PlantMonitoringAPI.DTOs
{
    public class PlantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string GroupName { get; set; } = "No Group";

        // latest reading
        public double? CurrentMoisture { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
