using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantMonitoringAPI.Models
{
    [Table("devices")]
    public class Device
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(17)]
        [Column("mac_address")]
        public string MacAddress { get; set; } 

        [Required]
        [Column("api_token")]
        public Guid ApiToken { get; set; } 

        // plant fk
        [Column("current_plant_id")]
        public int? CurrentPlantId { get; set; }

        public Plant? Plant { get; set; }
    }
}
