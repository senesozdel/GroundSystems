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
                // JSON verisini deserialize et
                var sensorData = JsonConvert.DeserializeObject<Sensor>(jsonData);

                if (sensorData == null)
                {
                    Console.WriteLine("Deserialize işlemi başarısız oldu veya boş veri geldi.");
                    return;
                }

                _logService.LogSensorEventAsync(sensorData.Id, $"Sensor Adı {sensorData.Name}, Sensor Tipi {sensorData.Type}, Değer {sensorData.CurrentValue} , Durum {sensorData.Status} ", LogLevel.Information);

                // Veritabanında sensör var mı kontrol et
                var existingSensor = await _sensorService.GetSensorByIdAsync(sensorData.Id);

                Sensor updatedSensor; // Event için kullanılacak sensör

                if (existingSensor == null)
                {
                    // Yeni sensör ekleme
                    await _sensorService.AddSensorAsync(sensorData);
                    updatedSensor = sensorData; // Yeni sensörü kullan
                }
                else
                {
                    // Mevcut sensörü güncelle
                    existingSensor.Status = sensorData.Status;
                    existingSensor.Timestamp = DateTime.Now;

                    // Gelen veriden diğer değerleri de güncelle
                    existingSensor.CurrentValue = sensorData.CurrentValue;
                    existingSensor.Name = sensorData.Name ?? existingSensor.Name;


                    await _sensorService.UpdateSensorAsync(existingSensor);
                    updatedSensor = existingSensor; // Güncellenmiş sensörü kullan
                }

                // Event'i her durumda doğru sensör ile tetikle
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
