using GroundSystems.Client.Models;
using GroundSystems.Server.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.Services.Network
{
    public interface INetworkService
    {
        Task<bool> ConnectAsync();
        Task<bool> SendSensorDataAsync(Sensor sensorData);
    }
}
