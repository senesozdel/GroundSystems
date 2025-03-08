using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Client.BackgroundJobs
{
    public interface ISensorSimulationJob
    {
        Task ExecuteSimulationAsync(CancellationToken cancellationToken);

    }
}
