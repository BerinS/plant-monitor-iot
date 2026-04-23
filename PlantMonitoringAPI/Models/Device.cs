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

        [MaxLength(17)]
        [Column("mac_address")]
        public string? MacAddress { get; set; }

        [Required]
        [MaxLength(30)]
        [Column("name")]
        public string? Name { get; set; }

        [Required]
        [Column("api_token")]
        public Guid ApiToken { get; set; } 

        // plant fk
        [Column("current_plant_id")]
        public int? CurrentPlantId { get; set; }

        public Plant? Plant { get; set; }

        [Column("group_id")]
        public int? GroupId { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }
    }
}
