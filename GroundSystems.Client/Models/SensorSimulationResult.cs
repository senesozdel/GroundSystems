using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.Models
{
    public class SensorSimulationResult
    {
        public int SensorId { get; set; }
        public SensorStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
