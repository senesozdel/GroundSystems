using GroundSystems.Server.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Services
{
    public interface ISensorDataProcessor
    {
        event EventHandler<Sensor> SensorUpdated;
        Task ProcessSensorDataAsync(string jsonData);
    }
}
