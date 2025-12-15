using System.ComponentModel.DataAnnotations;

namespace PlantMonitoringAPI.DTOs
{
    public class CreateSensorDataDto
    {
        // token sent by device
        [Required]
        public Guid Token { get; set; }

        // sensor reading
        [Required]
        [Range(0, 100)] 
        public double Value { get; set; }
    }
}
