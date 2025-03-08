using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;
using GroundSystems.Server.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _repository;
        private readonly ICustomLogService _logService;

        public SensorService(ISensorRepository repository, ICustomLogService logger)
        {
            _repository = repository;
            _logService = logger;
        }

        public async Task<Sensor> GetSensorByIdAsync(int id) =>
            await _repository.GetSensorByIdAsync(id);

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync() =>
            await _repository.GetAllSensorsAsync();

        public async Task UpdateSensorAsync(Sensor sensor)
        {
            try
            {
                if (sensor == null)
                    throw new ArgumentNullException(nameof(sensor));

                sensor.Timestamp = DateTime.UtcNow;

                await _repository.UpdateSensorAsync(sensor);

                await _logService.LogSensorEventAsync(
                   sensor.Id,
                   $"Sensor updated. Status: {sensor.Status}",
                   LogLevel.Information
           );
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(
                    ex,
                    "Sensor Update",
                    LogLevel.Error
                );
                throw;
            }
        }

        public async Task<IEnumerable<Sensor>> GetSensorsByStatusAsync(SensorStatus status)
        {
            var sensors = await _repository.GetAllSensorsAsync();
            return sensors.Where(s => s.Status == status).ToList();
        }


        public async Task<IEnumerable<Sensor>> GetCriticalSensorsAsync()
        {
            var sensors = await _repository.GetAllSensorsAsync();
            return sensors.Where(s => s.Status == SensorStatus.Critical);
        }


        public async Task<IEnumerable<Sensor>> GetSensorsNotReportingAsync(TimeSpan threshold)
        {
            var sensors = await _repository.GetAllSensorsAsync();
            var currentTime = DateTime.UtcNow;

            return sensors
                .Where(s => currentTime - s.Timestamp > threshold)
                .ToList();
        }

        public async Task AddSensorAsync(Sensor sensor)
        {
            try
            {
                if (sensor == null)
                    throw new ArgumentNullException(nameof(sensor));

                sensor.Timestamp = DateTime.UtcNow;

                await _repository.AddSensorAsync(sensor);


            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
