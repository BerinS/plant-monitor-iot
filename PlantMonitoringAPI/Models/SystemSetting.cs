using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantMonitoringAPI.Models
{
    [Table("system_settings")]
    public class SystemSetting
    {
        // Natural primary key 
        [Key]
        [MaxLength(100)]
        [Column("key")]
        public string Key { get; set; } = string.Empty;

        [Column("value")]
        public string? Value { get; set; }

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // valid setting keys for safety
    public static class SettingKeys
    {
        public const string MailEnabled = "mail_enabled";
        public const string MailHost = "mail_host";
        public const string MailPort = "mail_port";
        public const string MailUsername = "mail_username";
        public const string MailPassword = "mail_password";
        public const string MailFromAddress = "mail_from_address";
        public const string MailFromName = "mail_from_name";
        public const string MailToAddress = "mail_to_address";
    }
}