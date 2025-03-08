using GroundSystems.Client.Models;
using GroundSystems.Client.Services.Simulator;
using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;
using Microsoft.Extensions.Logging;

public class SensorSimulatorService : ISensorSimulatorService
{
    private readonly ILogger<SensorSimulatorService> _logger;
    private readonly Random _random = new Random();
    private readonly SensorDataGenerator _sensorDataGenerator; 

    public SensorSimulatorService(
        ILogger<SensorSimulatorService> logger,
        SensorDataGenerator sensorDataGenerator)
    {
        _logger = logger;
        _sensorDataGenerator = sensorDataGenerator;
    }

    public async Task<List<Sensor>> CreateSensors(SensorSimulationConfig config)
    {
        var sensors = Enumerable.Range(0, config.TotalSensors)
            .Select(_ => _sensorDataGenerator.GenerateSensor(
                (SensorType)_random.Next(0, Enum.GetValues(typeof(SensorType)).Length)
            ))
            .ToList();

        return sensors;
    }




}
