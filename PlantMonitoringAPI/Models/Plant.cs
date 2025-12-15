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
        public string Name { get; set; } 

        [MaxLength(100)]
        [Column("description")]
        public string Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // fk to Group
        [Column("group_id")]
        public int GroupId { get; set; }

        // link back to Group
        public PlantMonitoringAPI.Models.Group Group { get; set; }

        // one plant has many sensor readings
        public ICollection<SensorData> SensorReadings { get; set; } = new List<SensorData>();

        public Device? ActiveDevice { get; set; }
    }
}
