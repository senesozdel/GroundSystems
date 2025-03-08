using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.Models
{
    public class SensorLimits
    {
        public double LowCritical { get; set; }
        public double HighCritical { get; set; }
        public double LowWarning { get; set; }
        public double HighWarning { get; set; }
        public double LowNormal { get; set; }
        public double HighNormal { get; set; }
    }
}
