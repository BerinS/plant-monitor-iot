using System.ComponentModel.DataAnnotations;

namespace PlantMonitoringAPI.DTOs
{
    public class CreateSensorDataDto
    {
        [Required]
        public int DeviceId { get; set; }     

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        public double Value { get; set; }
    }
}