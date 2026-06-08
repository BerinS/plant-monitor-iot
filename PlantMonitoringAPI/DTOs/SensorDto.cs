namespace PlantMonitoringAPI.DTOs
{
    // Used for GET responses, no token exposed
    public class SensorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? MacAddress { get; set; }
        public int? CurrentPlantId { get; set; }
        public string? PlantName { get; set; }
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? Description { get; set; }
    }

    // Used only in the CreateSensor response, returned exactly once
    public class CreatedSensorDto : SensorDto
    {
        public string PlainApiToken { get; set; } = string.Empty;
    }
}
