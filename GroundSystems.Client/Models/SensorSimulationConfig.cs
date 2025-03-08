using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.Models
{
    public class SensorSimulationConfig
    {
        public int TotalSensors { get; set; } = 5;
        public int SimulationDurationMinutes { get; set; } = 60;
        public int DataSendIntervalSeconds { get; set; } = 2;
        public bool EnableRandomFailures { get; set; } = true;
    }
}
