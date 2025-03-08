using GroundSystems.Server.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroundSystems.Server.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<Sensor> Sensors { get; set; }

        public AppDbContext() : base("name=DefaultConnection")
        {
            Database.SetInitializer<AppDbContext>(null);
        }

    }
}
