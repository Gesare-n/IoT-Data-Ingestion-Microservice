using Microsoft.EntityFrameworkCore;

namespace IoTDataIngestion
{
    /// <summary>
    /// Database context for the IoT Data Ingestion application
    /// Manages the SQLite database connection and entity mappings
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for sensor readings - enables LINQ queries against the database
        /// </summary>
        public DbSet<SensorReading> SensorReadings => Set<SensorReading>();

        /// <summary>
        /// Configure the model for the SensorReading entity
        /// </summary>
        /// <param name="modelBuilder">Model builder used to configure the entity</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DeviceId).IsRequired();
                entity.Property(e => e.Temperature).IsRequired();
                entity.Property(e => e.Humidity).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}