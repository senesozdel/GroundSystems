using GroundSystems.Server.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Repositories
{
    public interface ISensorRepository
    {
        Task<Sensor> AddSensorAsync(Sensor sensor);
        Task<Sensor> UpdateSensorAsync(Sensor sensor);
        Task<IEnumerable<Sensor>> GetAllSensorsAsync();

        Task<Sensor> GetSensorByIdAsync(int id);
        Task DeleteSensorAsync(int id);
    }
}
