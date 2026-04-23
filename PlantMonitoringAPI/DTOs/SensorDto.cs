namespace PlantMonitoringAPI.DTOs
{
    public class SensorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? MacAddress { get; set; } = string.Empty;
        public Guid ApiToken { get; set; }
        public int? CurrentPlantId { get; set; }
        public int? GroupId { get; set; }
        public string? Description { get; set; }
    }
}
