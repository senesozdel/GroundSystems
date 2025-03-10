    using GroundSystems.Client.Models;
    using GroundSystems.Client.Services;
    using GroundSystems.Client.Services.Network;
using GroundSystems.Client.Services.Simulator;
using GroundSystems.Server.Models.Entities;
    using GroundSystems.Server.Models.Enums;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    namespace GroundSystems.Client.BackgroundJobs
    {
        public class SensorSimulationJob : ISensorSimulationJob
        {

            private readonly ISensorSimulatorService _simulatorService;
            private readonly INetworkService _networkService;
            private readonly ILogger<SensorSimulationJob> _logger;
            private List<Sensor> _sensors; 
            private readonly Random _random = new Random();
            private readonly ISensorRangeService _sensorRangeService;
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
            private readonly List<Task> _sensorTasks = new List<Task>();
            public SensorSimulationJob(
               ISensorSimulatorService simulatorService,
               INetworkService networkService,
               ISensorRangeService sensorRangeService,
               ILogger<SensorSimulationJob> logger)
            {
                _simulatorService = simulatorService;
                _networkService = networkService;
                _sensorRangeService = sensorRangeService;
                _logger = logger;
            }

            public async Task ExecuteSimulationAsync(CancellationToken cancellationToken)
            {
                if (_sensors == null)
                {

                    _sensors = await _simulatorService.CreateSensors(new SensorSimulationConfig { TotalSensors = 5});
                    _logger.LogInformation($"{_sensors.Count} sensör başarıyla oluşturuldu");
                }

                foreach (var sensor in _sensors)
                {
                    var task = StartSensorReportingTaskAsync(sensor, _cancellationTokenSource.Token);
                    _sensorTasks.Add(task);
                }

                _logger.LogInformation("Tüm sensör görevleri başlatıldı");
            }

            private async Task StartSensorReportingTaskAsync(Sensor sensor, CancellationToken cancellationToken)
            {
                _logger.LogInformation($"Sensör {sensor.Id} raporlama görevi başlatıldı");

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await SendSensorData(sensor, cancellationToken);
                        await Task.Delay(2000, cancellationToken); 
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation($"Sensör {sensor.Id} raporlama işlemi iptal edildi");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Sensör {sensor.Id} raporlama sırasında hata oluştu");
                    }
                }
            }

            public void UpdateSensor(Sensor sensor)
            {
                double newValue = GenerateRandomValueForSensorType(sensor.Type);
                sensor.CurrentValue = Math.Round(newValue, 2);

                try
                {
                    sensor.Status = _sensorRangeService.CheckStatus(sensor.Type, sensor.CurrentValue);
                }
                catch (KeyNotFoundException)
                {
                    var statusValues = Enum.GetValues(typeof(SensorStatus));
                    sensor.Status = (SensorStatus)statusValues.GetValue(_random.Next(statusValues.Length));
                }

                _logger.LogInformation($"Sensör {sensor.Id} güncellendi: Tip={sensor.Type}, Durum={sensor.Status}, Değer={sensor.CurrentValue}");
            }
            private double GenerateRandomValueForSensorType(SensorType sensorType)
            {
                switch (sensorType)
                {
                    case SensorType.Temperature:
                        return _random.NextDouble() * 150 - 50; // -50 ile 100 derece arası

                    case SensorType.Humidity:
                        return _random.NextDouble() * 100; // %0-100 arası

                    case SensorType.Pressure:
                        return _random.NextDouble() * 800 + 100; // 100-900 hPa

                    case SensorType.Vibration:
                        return _random.NextDouble() * 50; // 0-50 birim

                    default:
                        return _random.NextDouble() * 1000;
                }
            }

            private async Task SendSensorData(Sensor sensor, CancellationToken cancellationToken)
            {


                var result = await _networkService.SendSensorDataAsync(sensor);
                if (!result)
                {
                    _logger.LogWarning($"Sensör {sensor.Id} verisi gönderilemedi!");
                }
            }

            public IReadOnlyList<Sensor> GetSensors() => _sensors?.AsReadOnly();

            public async Task UpdateAndSendSensorAsync(Sensor sensor, double? newValue = null)
            {
                if (newValue.HasValue)
                {
                    double oldValue = sensor.CurrentValue;
                    sensor.CurrentValue = newValue.Value;

                    try
                    {
                        sensor.Status = _sensorRangeService.CheckStatus(sensor.Type, sensor.CurrentValue);
                    }
                    catch (KeyNotFoundException)
                    {
                        var statusValues = Enum.GetValues(typeof(SensorStatus));
                        sensor.Status = (SensorStatus)_random.Next(statusValues.Length);
                    }
                }
                else
                {
                    UpdateSensor(sensor);
                }

                await SendSensorData(sensor, CancellationToken.None);
            }


        }
    }
