using IoTDataDashboard.Models;
using System.Net.Http.Json;

namespace IoTDataDashboard.Services
{
    /// <summary>
    /// Service for fetching sensor data from the backend API
    /// </summary>
    public class SensorDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SensorDataService> _logger;

        public SensorDataService(HttpClient httpClient, ILogger<SensorDataService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Fetches the latest sensor readings from the backend API
        /// </summary>
        /// <returns>List of sensor readings or empty list if error occurs</returns>
        public async Task<List<SensorReading>> GetLatestReadingsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching sensor readings from {Url}", _httpClient.BaseAddress);
                var response = await _httpClient.GetAsync("api/sensorreadings");
                
                if (response.IsSuccessStatusCode)
                {
                    var readings = await response.Content.ReadFromJsonAsync<List<SensorReading>>();
                    _logger.LogInformation("Successfully fetched {Count} sensor readings", readings?.Count ?? 0);
                    return readings ?? new List<SensorReading>();
                }
                else
                {
                    _logger.LogError("Failed to fetch sensor readings. Status code: {StatusCode}", response.StatusCode);
                    return new List<SensorReading>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sensor readings");
                return new List<SensorReading>();
            }
        }

        /// <summary>
        /// Fetches sensor readings for a specific device
        /// </summary>
        /// <param name="deviceId">The ID of the device</param>
        /// <param name="limit">Number of readings to return (default: 10)</param>
        /// <returns>List of sensor readings for the specified device</returns>
        public async Task<List<SensorReading>> GetReadingsByDeviceAsync(string deviceId, int limit = 10)
        {
            try
            {
                _logger.LogInformation("Fetching sensor readings for device {DeviceId}", deviceId);
                var response = await _httpClient.GetAsync($"api/sensorreadings/device/{deviceId}?limit={limit}");
                
                if (response.IsSuccessStatusCode)
                {
                    var readings = await response.Content.ReadFromJsonAsync<List<SensorReading>>();
                    _logger.LogInformation("Successfully fetched {Count} sensor readings for device {DeviceId}", readings?.Count ?? 0, deviceId);
                    return readings ?? new List<SensorReading>();
                }
                else
                {
                    _logger.LogError("Failed to fetch sensor readings for device {DeviceId}. Status code: {StatusCode}", deviceId, response.StatusCode);
                    return new List<SensorReading>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sensor readings for device {DeviceId}", deviceId);
                return new List<SensorReading>();
            }
        }

        /// <summary>
        /// Fetches all unique device IDs
        /// </summary>
        /// <returns>List of device IDs</returns>
        public async Task<List<string>> GetDeviceIdsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching device IDs");
                var response = await _httpClient.GetAsync("api/sensorreadings/devices");
                
                if (response.IsSuccessStatusCode)
                {
                    var deviceIds = await response.Content.ReadFromJsonAsync<List<string>>();
                    _logger.LogInformation("Successfully fetched {Count} device IDs", deviceIds?.Count ?? 0);
                    return deviceIds ?? new List<string>();
                }
                else
                {
                    _logger.LogError("Failed to fetch device IDs. Status code: {StatusCode}", response.StatusCode);
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching device IDs");
                return new List<string>();
            }
        }
    }
}