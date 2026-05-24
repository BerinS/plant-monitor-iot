using System.ComponentModel.DataAnnotations;

namespace PlantMonitoringAPI.DTOs
{
    public class UpdatePlantDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? GroupId { get; set; }

        [Range(0, 100, ErrorMessage = "Threshold must be between 0 and 100.")]
        public int? MoistureThreshold { get; set; }
    }
}