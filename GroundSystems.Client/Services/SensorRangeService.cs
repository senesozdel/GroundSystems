using GroundSystems.Client.Models;
using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.Services
{
    public class SensorRangeService : ISensorRangeService
    {
        private readonly Dictionary<SensorType, SensorLimits> _limits;

        public SensorRangeService()
        {
            _limits = new Dictionary<SensorType, SensorLimits>
        {
            {
                SensorType.Temperature, new SensorLimits
                {
                    LowCritical = -50,
                    HighCritical = 100,
                    LowWarning = -25,
                    HighWarning = 85,
                    LowNormal = 0,
                    HighNormal = 70
                }
            },
            {
                SensorType.Humidity, new SensorLimits
                {
                    LowCritical = 10,
                    HighCritical = 90,
                    LowWarning = 20,
                    HighWarning = 80,
                    LowNormal = 30,
                    HighNormal = 70
                }
            },
            {
                SensorType.Pressure, new SensorLimits
                {
                    LowCritical = 100,
                    HighCritical = 900,
                    LowWarning = 200,
                    HighWarning = 800,
                    LowNormal = 300,
                    HighNormal = 700
                }
            },
            {
                SensorType.Vibration, new SensorLimits
                {
                    LowCritical = 0,
                    HighCritical = 50,
                    LowWarning = 0,
                    HighWarning = 25,
                    LowNormal = 0,
                    HighNormal = 15
                }
            }
        };
        }

        public SensorStatus CheckStatus(SensorType type, double value)
        {
            var limit = _limits[type];

            if (value <= limit.LowCritical || value >= limit.HighCritical)
                return SensorStatus.Critical;

            if (value <= limit.LowWarning || value >= limit.HighWarning)
                return SensorStatus.Warning;

            if (value >= limit.LowNormal && value <= limit.HighNormal)
                return SensorStatus.Nominal;

            return SensorStatus.Nominal;
        }
    }


}
