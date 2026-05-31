using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Group> Groups { get; set; }
    public DbSet<Plant> Plants { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<SensorData> SensorData { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<EventLog> EventLog { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Groups -> Plants (One-to-Many)
        modelBuilder.Entity<Plant>()
            .HasOne(p => p.Group)
            .WithMany(g => g.Plants)
            .HasForeignKey(p => p.GroupId)
            .OnDelete(DeleteBehavior.SetNull); // group deletion unassigns plants

        // Plants -> SensorData (One-to-Many)
        modelBuilder.Entity<SensorData>()
            .HasOne(sd => sd.Plant)
            .WithMany(p => p.SensorReadings)
            .HasForeignKey(sd => sd.PlantId)
            .OnDelete(DeleteBehavior.Cascade); // delete sensor data if plant is deleted

        // Devices -> Plants (One-to-One / Zero-to-One)
        // A device has one optional plant. A plant has one optional device.
        modelBuilder.Entity<Device>()
            .HasOne(d => d.Plant)
            .WithOne(p => p.ActiveDevice)
            .HasForeignKey<Device>(d => d.CurrentPlantId)
            .OnDelete(DeleteBehavior.SetNull); // if plant is deleted device is unassigned

        // MacAddress partial unique index, nulls excluded
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.MacAddress)
            .IsUnique()
            .HasFilter("mac_address IS NOT NULL");

        modelBuilder.Entity<SensorData>()
            .HasIndex(sd => new { sd.PlantId, sd.MeasuredAt }); // Composite index for graphs
    }
}