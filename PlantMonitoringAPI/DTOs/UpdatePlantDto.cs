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

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid group")]
        public int GroupId { get; set; }
    }
}
