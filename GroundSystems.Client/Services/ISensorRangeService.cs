using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.Services
{
    public interface ISensorRangeService
    {
        SensorStatus CheckStatus(SensorType type, double value);
    }
}
