using GroundSystems.Client.Services;
using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;

public class SensorDataGenerator
{
    private readonly Random _random = new();
    private readonly ISensorRangeService _sensorRangeService;

    public SensorDataGenerator(ISensorRangeService sensorRangeService)
    {
        _sensorRangeService = sensorRangeService;
    }

    public Sensor GenerateSensor(SensorType type)
    {
        var value = GenerateSensorValue(type);
        return new Sensor
        {
            Id = _random.Next(1, 1000),
            Name = $"{type} Sensor",
            Type = type,
            CurrentValue = value,
            Status = _sensorRangeService.CheckStatus(type, value),
            Timestamp = DateTime.UtcNow
        };
    }

    private double GenerateSensorValue(SensorType type) => type switch
    {
        SensorType.Temperature => Math.Round(_random.NextDouble() * 100, 2),
        SensorType.Pressure => Math.Round(_random.NextDouble() * 1000, 2),
        SensorType.Humidity => Math.Round(_random.NextDouble() * 100, 2),
        SensorType.Vibration => Math.Round(_random.NextDouble() * 10, 2),
        _ => 0
    };
}
