# IoT Data Ingestion Microservice

A high-performance ASP.NET Core Minimal API that acts as a backend for an IoT solution. It subscribes to an MQTT topic, deserializes incoming JSON data, and persists it to a SQLite database.

## Features

- **ASP.NET Core Minimal API** (.NET 8)
- **Entity Framework Core** with SQLite for data persistence
- **MQTTnet** library for MQTT messaging
- Background service for MQTT connection management
- REST API endpoint to retrieve sensor readings

## Project Structure

```
IoTDataIngestion/
├── SensorReading.cs          # Data model for sensor readings
├── ApplicationDbContext.cs   # EF Core database context
├── MqttService.cs            # Background service for MQTT communication
├── SensorReadingsController.cs # API controller for sensor data
├── Program.cs                # Application entry point
└── appsettings.json          # Configuration file
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio, VS Code, or JetBrains Rider (optional)

### Installation

1. Create the project:
   ```bash
   dotnet new webapi -n IoTDataIngestion
   cd IoTDataIngestion
   ```

2. Install required packages:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add package MQTTnet
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## How It Works

1. **MQTT Connection**: The application connects to the public HiveMQ MQTT broker at `broker.hivemq.com:1883`
2. **Topic Subscription**: It subscribes to the `/sensors/data` topic
3. **Message Processing**: When a message arrives, it's deserialized from JSON to a SensorReading object
4. **Data Persistence**: The sensor reading is saved to a SQLite database
5. **API Access**: The latest 10 readings can be retrieved via `GET /api/sensorreadings`

## API Endpoints

- `GET /` - Health check endpoint
- `GET /api/sensorreadings` - Returns the 10 most recent sensor readings

## Data Model

The SensorReading model includes:
- `Id` (int) - Unique identifier
- `DeviceId` (string) - Identifier of the device
- `Temperature` (double) - Temperature reading in Celsius
- `Humidity` (double) - Humidity reading in percentage
- `Timestamp` (DateTimeOffset) - When the reading was taken

## Testing

You can test the service using the included TestMqttPublisher project or any MQTT client that can publish to the `/sensors/data` topic with JSON payloads in this format:

```json
{
  "DeviceId": "device-001",
  "Temperature": 23.5,
  "Humidity": 65.2,
  "Timestamp": "2023-10-29T10:30:00.000Z"
}
```

## Architecture

This microservice follows modern .NET practices:

1. **Separation of Concerns**: Each class has a single responsibility
2. **Dependency Injection**: Services are registered and injected where needed
3. **Background Processing**: MQTT communication happens in a background service
4. **Data Access**: EF Core provides ORM capabilities with SQLite
5. **Logging**: Built-in .NET logging for monitoring and debugging

## Technologies Used

- ASP.NET Core Minimal API
- Entity Framework Core
- SQLite
- MQTTnet
- System.Text.Json

## License

This project is open source.