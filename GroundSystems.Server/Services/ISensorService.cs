using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Services
{
    public interface ISensorService
    {
        Task<Sensor> GetSensorByIdAsync(int id);
        Task<IEnumerable<Sensor>> GetAllSensorsAsync();
        Task UpdateSensorAsync(Sensor sensor);

        Task AddSensorAsync(Sensor sensor);

        Task<IEnumerable<Sensor>> GetCriticalSensorsAsync();

        Task<IEnumerable<Sensor>> GetSensorsByStatusAsync(SensorStatus status);
        Task<IEnumerable<Sensor>> GetSensorsNotReportingAsync(TimeSpan threshold);

    }
}
