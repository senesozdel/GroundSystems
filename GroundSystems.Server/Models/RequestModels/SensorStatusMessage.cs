﻿using GroundSystems.Server.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Models.RequestModels
{
    public class SensorStatusMessage
    {
        public int SensorId { get; set; }
        public SensorStatus Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
