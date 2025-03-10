using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;
using GroundSystems.Server.Models.RequestModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GroundSystems.Server.Services
{
    public class SensorDataProcessor : ISensorDataProcessor, IMessageHandler
    {
        private readonly ISensorService _sensorService;
        private readonly ICustomLogService _logService;

        public event EventHandler<Sensor> SensorUpdated;


        public SensorDataProcessor(ISensorService sensorService, ICustomLogService logService)
        {
            _sensorService = sensorService;
            _logService = logService;
        }

        public void HandleMessage(string jsonData)
        {
            _ = ProcessSensorDataAsync(jsonData);
        }

        public async Task ProcessSensorDataAsync(string jsonData)
        {
            try
            {
                var sensorData = JsonConvert.DeserializeObject<Sensor>(jsonData);

                if (sensorData == null)
                {
                    Console.WriteLine("Deserialize işlemi başarısız oldu veya boş veri geldi.");
                    return;
                }

                _logService.LogSensorEventAsync(sensorData.Id, $"Sensor Adı {sensorData.Name}, Sensor Tipi {sensorData.Type}, Değer {sensorData.CurrentValue} , Durum {sensorData.Status} ", LogLevel.Information);

                var existingSensor = await _sensorService.GetSensorByIdAsync(sensorData.Id);

                Sensor updatedSensor; 

                if (existingSensor == null)
                {
                    await _sensorService.AddSensorAsync(sensorData);
                    updatedSensor = sensorData; 
                }
                else
                {
                    existingSensor.Status = sensorData.Status;
                    existingSensor.Timestamp = DateTime.Now;

                    existingSensor.CurrentValue = sensorData.CurrentValue;
                    existingSensor.Name = sensorData.Name ?? existingSensor.Name;


                    await _sensorService.UpdateSensorAsync(existingSensor);
                    updatedSensor = existingSensor; 
                }

                if (updatedSensor != null)
                {
                    SensorUpdated?.Invoke(this, updatedSensor);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON Deserialize hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"İşlem sırasında beklenmeyen hata: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }



    }
}
