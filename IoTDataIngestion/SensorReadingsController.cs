using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTDataIngestion
{
    /// <summary>
    /// Controller for sensor readings API endpoints
    /// Provides RESTful access to sensor data stored in the database
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SensorReadingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SensorReadingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the 10 most recent sensor readings
        /// </summary>
        /// <returns>List of the 10 most recent sensor readings</returns>
        [HttpGet]
        public ActionResult<IEnumerable<SensorReading>> GetLatestReadings()
        {
            // Note: Using AsEnumerable() to switch to client-side evaluation
            // because SQLite has limitations with ordering by DateTimeOffset
            var latestReadings = _context.SensorReadings
                .AsEnumerable()
                .OrderByDescending(r => r.Timestamp.UtcDateTime)
                .Take(10)
                .ToList();

            return Ok(latestReadings);
        }

        /// <summary>
        /// Gets sensor readings for a specific device
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <param name="limit">Number of readings to return (default: 10)</param>
        /// <returns>List of sensor readings for the specified device</returns>
        [HttpGet("device/{deviceId}")]
        public ActionResult<IEnumerable<SensorReading>> GetReadingsByDevice(string deviceId, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return BadRequest("Device ID is required");
            }

            var readings = _context.SensorReadings
                .Where(r => r.DeviceId == deviceId)
                .AsEnumerable()
                .OrderByDescending(r => r.Timestamp.UtcDateTime)
                .Take(limit)
                .ToList();

            return Ok(readings);
        }

        /// <summary>
        /// Gets sensor readings within a specific date range
        /// </summary>
        /// <param name="startDate">Start date for the range</param>
        /// <param name="endDate">End date for the range</param>
        /// <returns>List of sensor readings within the specified date range</returns>
        [HttpGet("range")]
        public ActionResult<IEnumerable<SensorReading>> GetReadingsByDateRange(
            [FromQuery] DateTimeOffset startDate, 
            [FromQuery] DateTimeOffset endDate)
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var readings = _context.SensorReadings
                .Where(r => r.Timestamp >= startDate && r.Timestamp <= endDate)
                .AsEnumerable()
                .OrderByDescending(r => r.Timestamp.UtcDateTime)
                .ToList();

            return Ok(readings);
        }

        /// <summary>
        /// Gets statistics for all sensor readings
        /// </summary>
        /// <returns>Statistics including count, average temperature, and average humidity</returns>
        [HttpGet("statistics")]
        public ActionResult<object> GetStatistics()
        {
            var allReadings = _context.SensorReadings.AsEnumerable().ToList();
            
            if (!allReadings.Any())
            {
                return Ok(new
                {
                    TotalReadings = 0,
                    AverageTemperature = 0.0,
                    AverageHumidity = 0.0,
                    MinTemperature = 0.0,
                    MaxTemperature = 0.0,
                    MinHumidity = 0.0,
                    MaxHumidity = 0.0
                });
            }

            return Ok(new
            {
                TotalReadings = allReadings.Count,
                AverageTemperature = allReadings.Average(r => r.Temperature),
                AverageHumidity = allReadings.Average(r => r.Humidity),
                MinTemperature = allReadings.Min(r => r.Temperature),
                MaxTemperature = allReadings.Max(r => r.Temperature),
                MinHumidity = allReadings.Min(r => r.Humidity),
                MaxHumidity = allReadings.Max(r => r.Humidity)
            });
        }

        /// <summary>
        /// Gets a list of all unique device IDs
        /// </summary>
        /// <returns>List of device IDs</returns>
        [HttpGet("devices")]
        public ActionResult<IEnumerable<string>> GetDeviceIds()
        {
            var deviceIds = _context.SensorReadings
                .Select(r => r.DeviceId)
                .Distinct()
                .ToList();

            return Ok(deviceIds);
        }
    }
}