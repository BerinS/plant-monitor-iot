namespace PlantMonitoringAPI.DTOs
{
    public class DeviceHealthDto
    {
        public int Id { get; set; }
        public string MacAddress { get; set; } = string.Empty; // using MAC as name
        public string AssignedPlant { get; set; } = "Unassigned";
        public DateTime? LastContact { get; set; }
    }
}
