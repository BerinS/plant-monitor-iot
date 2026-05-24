using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantMonitoringAPI.Models
{
    [Table("plants")]
    public class Plant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("group_id")]
        public int? GroupId { get; set; }

        // if null the no check runs
        [Range(0, 100)]
        [Column("moisture_threshold")]
        public int? MoistureThreshold { get; set; }

        public Group? Group { get; set; }

        public ICollection<SensorData> SensorReadings { get; set; } = new List<SensorData>();

        public Device? ActiveDevice { get; set; }
    }
}