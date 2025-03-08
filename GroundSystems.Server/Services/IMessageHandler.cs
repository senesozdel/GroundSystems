using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Services
{
    public interface IMessageHandler
    {
        void HandleMessage(string message);
    }

}
