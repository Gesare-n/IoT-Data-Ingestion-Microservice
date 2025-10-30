using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class TestMqttPublisher
{
    static async Task Main(string[] args)
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer("broker.hivemq.com", 1883)
            .Build();

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        Console.WriteLine("Connected to MQTT broker");

        // Send test messages
        for (int i = 0; i < 5; i++)
        {
            var sensorReading = new
            {
                DeviceId = $"Device-{i}",
                Temperature = 20.0 + i,
                Humidity = 50.0 + i,
                Timestamp = DateTimeOffset.Now
            };

            var json = JsonSerializer.Serialize(sensorReading);
            var message = new MqttApplicationMessageBuilder()
                .WithTopic("/sensors/data")
                .WithPayload(Encoding.UTF8.GetBytes(json))
                .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None);
            Console.WriteLine($"Published: {json}");

            await Task.Delay(2000);
        }

        await mqttClient.DisconnectAsync();
        Console.WriteLine("Disconnected from MQTT broker");
    }
}