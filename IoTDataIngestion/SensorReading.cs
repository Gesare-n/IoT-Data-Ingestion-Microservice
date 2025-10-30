using System.ComponentModel.DataAnnotations;

namespace IoTDataIngestion
{
    /// <summary>
    /// Represents a sensor reading from an IoT device
    /// </summary>
    public class SensorReading
    {
        /// <summary>
        /// Unique identifier for the reading
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Identifier of the device that sent the reading
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string DeviceId { get; set; } = string.Empty;
        
        /// <summary>
        /// Temperature reading in Celsius
        /// </summary>
        [Range(-273.15, 1000)]
        public double Temperature { get; set; }
        
        /// <summary>
        /// Humidity reading in percentage
        /// </summary>
        [Range(0, 100)]
        public double Humidity { get; set; }
        
        /// <summary>
        /// Timestamp when the reading was taken
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }
    }
}