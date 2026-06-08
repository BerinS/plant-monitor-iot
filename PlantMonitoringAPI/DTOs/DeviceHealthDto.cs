namespace PlantMonitoringAPI.DTOs
{
    public class DeviceHealthDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string AssignedPlant { get; set; } = "Unassigned";
        public DateTime? LastContact { get; set; }
    }
}
