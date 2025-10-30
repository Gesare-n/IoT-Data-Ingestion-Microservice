using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace IoTDataIngestion
{
    /// <summary>
    /// Background service that handles MQTT communication
    /// Connects to an MQTT broker, subscribes to sensor data topics, 
    /// and saves incoming readings to the database
    /// </summary>
    public class MqttService : BackgroundService
    {
        private readonly ILogger<MqttService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMqttClient _mqttClient;
        private readonly MqttFactory _mqttFactory;

        public MqttService(ILogger<MqttService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _mqttFactory = new MqttFactory();
            _mqttClient = _mqttFactory.CreateMqttClient();
        }

        /// <summary>
        /// Main execution method for the background service
        /// Establishes MQTT connection and handles incoming messages
        /// </summary>
        /// <param name="stoppingToken">Token to signal service shutdown</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Configure MQTT client options to connect to HiveMQ public broker
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("broker.hivemq.com", 1883) // HiveMQ public broker
                .Build();

            // Setup message handler for incoming MQTT messages
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var payload = e.ApplicationMessage.PayloadSegment;
                var message = Encoding.UTF8.GetString(payload);
                _logger.LogInformation("Received message: {Message}", message);

                try
                {
                    // Deserialize the JSON message to SensorReading
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var sensorReading = JsonSerializer.Deserialize<SensorReading>(message, options);

                    if (sensorReading != null)
                    {
                        // Validate the sensor reading
                        if (ValidateSensorReading(sensorReading, out var validationErrors))
                        {
                            _logger.LogInformation("Sensor Data - Device: {DeviceId}, Temp: {Temperature}, Humidity: {Humidity}, Time: {Timestamp}",
                                sensorReading.DeviceId, sensorReading.Temperature, sensorReading.Humidity, sensorReading.Timestamp);
                            
                            // Save to database using a scoped service
                            using var scope = _scopeFactory.CreateScope();
                            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                            dbContext.SensorReadings.Add(sensorReading);
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation("Saved sensor reading to database");
                        }
                        else
                        {
                            _logger.LogWarning("Invalid sensor reading received: {Errors}", string.Join(", ", validationErrors));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", message);
                }
            };

            // Connect to the MQTT broker
            try
            {
                await _mqttClient.ConnectAsync(mqttClientOptions, stoppingToken);
                _logger.LogInformation("Connected to MQTT broker");

                // Subscribe to the sensor data topic
                var mqttSubscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter("/sensors/data")
                    .Build();

                await _mqttClient.SubscribeAsync(mqttSubscribeOptions, stoppingToken);
                _logger.LogInformation("Subscribed to topic: /sensors/data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to MQTT broker");
            }

            // Keep the service running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        /// <summary>
        /// Validates a sensor reading object
        /// </summary>
        /// <param name="sensorReading">The sensor reading to validate</param>
        /// <param name="errors">List of validation errors if any</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateSensorReading(SensorReading sensorReading, out List<string> errors)
        {
            errors = new List<string>();
            
            // Check for required fields
            if (string.IsNullOrWhiteSpace(sensorReading.DeviceId))
            {
                errors.Add("DeviceId is required");
            }
            else if (sensorReading.DeviceId.Length > 50)
            {
                errors.Add("DeviceId must be 50 characters or less");
            }
            
            // Check temperature range
            if (sensorReading.Temperature < -273.15 || sensorReading.Temperature > 1000)
            {
                errors.Add("Temperature must be between -273.15 and 1000 Celsius");
            }
            
            // Check humidity range
            if (sensorReading.Humidity < 0 || sensorReading.Humidity > 100)
            {
                errors.Add("Humidity must be between 0 and 100 percent");
            }
            
            // Check timestamp is not in the future
            if (sensorReading.Timestamp > DateTimeOffset.Now.AddMinutes(5))
            {
                errors.Add("Timestamp cannot be in the future");
            }
            
            return errors.Count == 0;
        }

        /// <summary>
        /// Cleanup method called when the service is stopping
        /// Disconnects from the MQTT broker
        /// </summary>
        /// <param name="cancellationToken">Token to signal shutdown</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT service");
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            await base.StopAsync(cancellationToken);
        }
    }
}