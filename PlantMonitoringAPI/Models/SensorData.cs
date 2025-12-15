using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantMonitoringAPI.Models
{
    [Table("sensor_data")]
    public class SensorData
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("moisture_value")]
        public double MoistureValue { get; set; } 

        [Required]
        [Column("measured_at")]
        public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

        // fk to plant
        [Column("plant_id")]
        public int PlantId { get; set; }

        public Plant Plant { get; set; }
    }
}
