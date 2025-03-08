using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Models.Entities
{
    public class Sensor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SensorType Type { get; set; }
        public double CurrentValue { get; set; }
        public SensorStatus Status { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;


    }
}
