using System.ComponentModel.DataAnnotations;

namespace PlantMonitoringAPI.DTOs
{
    public class CreatePlantDto
    {
        [Required(ErrorMessage = "A plant must have a name.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description is too long - max 500")]
        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid group")]
        public int GroupId { get; set; }
    }
}
