using GroundSystems.Server.Context;
using GroundSystems.Server.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GroundSystems.Server.Repositories
{
    public class SensorRepository : ISensorRepository
    {
        private readonly AppDbContext _context;

        public SensorRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Sensor> AddSensorAsync(Sensor sensor)
        {
            if (sensor == null)
                throw new ArgumentNullException(nameof(sensor));

            _context.Sensors.Add(sensor);
            await _context.SaveChangesAsync();
            return sensor;
        }

        public async Task<Sensor> UpdateSensorAsync(Sensor sensor)
        {
            if (sensor == null)
                throw new ArgumentNullException(nameof(sensor));

            _context.Entry(sensor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return sensor;
        }

        public async Task<IEnumerable<Sensor>> GetAllSensorsAsync()
        {
            return await _context.Sensors
                .Where(s => !s.IsDeleted)
                .ToListAsync();
        }

        public async Task<Sensor> GetSensorByIdAsync(int id)
        {
            return await _context.Sensors
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task DeleteSensorAsync(int id)
        {
            var sensor = await _context.Sensors
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (sensor == null)
                return;

            sensor.IsDeleted = true;

            _context.Entry(sensor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SensorExistsAsync(int id)
        {
            return await _context.Sensors
                .AnyAsync(s => s.Id == id && !s.IsDeleted);
        }
    }
}
