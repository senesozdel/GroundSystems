using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Services
{
    public interface INetworkService
    {
        Task StartServer();
        Task StopServer();
        bool IsRunning { get; }
        event EventHandler<string> DataReceived;
    }
}
