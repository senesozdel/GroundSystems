using GroundSystems.Client.BackgroundJobs;
using GroundSystems.Client.Models;
using GroundSystems.Client.Services;
using GroundSystems.Client.Services.Network;
using GroundSystems.Client.Services.Simulator;
using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GroundSystems.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Uydu Sensör İzleme Sistemi ===");

            Console.WriteLine("Server'ın başlaması bekleniyor...");
            // 5 saniye bekle
            System.Threading.Thread.Sleep(5000);

            // Servis sağlayıcıyı yapılandır
            var serviceProvider = ConfigureServices();

            try
            {
                // SensorSimulationJob oluştur
                var sensorJob = serviceProvider.GetRequiredService<SensorSimulationJob>();
                var networkService = serviceProvider.GetRequiredService<INetworkService>();

                // Server'a bağlanma
                Console.WriteLine("Sunucuya bağlanılıyor...");
                bool connected = await networkService.ConnectAsync();

                if (!connected)
                {
                    Console.WriteLine("Sunucuya bağlantı kurulamadı!");
                    return;
                }

                Console.WriteLine("Sunucuya bağlantı başarılı!");

                // Sensör simülasyonunu başlat
                Console.WriteLine("Sensör simülasyonu başlatılıyor...");
                await sensorJob.ExecuteSimulationAsync(CancellationToken.None);

                // Kullanıcı arayüzünü başlat
                await RunUserInterfaceAsync(sensorJob);
            }
            catch (Exception ex)
            {
               
            }

            Console.WriteLine("Program sonlandırılıyor...");
        }

        private static async Task RunUserInterfaceAsync(SensorSimulationJob sensorJob)
        {
            bool exit = false;

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== Uydu Sensör İzleme Sistemi ===");
                Console.WriteLine("1. Sensörleri Görüntüle");
                Console.WriteLine("2. Sensör Güncelle");
                Console.WriteLine("3. Çıkış");
                Console.Write("\nSeçiminiz: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DisplaySensors(sensorJob);
                        break;

                    case "2":
                        await UpdateSensorMenuAsync(sensorJob);
                        break;

                    case "3":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("Geçersiz seçim!");
                        break;
                }

                if (!exit)
                {
                    Console.WriteLine("\nDevam etmek için bir tuşa basın...");
                    Console.ReadKey();
                }
            }
        }

        private static void DisplaySensors(SensorSimulationJob sensorJob)
        {
            var sensors = sensorJob.GetSensors();

            if (sensors == null || !sensors.Any())
            {
                Console.WriteLine("Henüz hiç sensör bulunmuyor.");
                return;
            }

            Console.WriteLine("\n=== Mevcut Sensörler ===");
            Console.WriteLine("ID\tTip\t\tDurum\t\tDeğer");
            Console.WriteLine("--------------------------------------------------");

            foreach (var sensor in sensors)
            {
                string statusText = GetStatusDisplayText(sensor.Status);
                string statusColor = GetStatusColor(sensor.Status);
                string sensorType = sensor.Type.ToString();

                // Uzun tip adlarını düzgün göstermek için
                if (sensorType.Length < 8)
                    sensorType += "\t";

                // Durum renklerini ayarla
                Console.ForegroundColor = GetConsoleColorForStatus(sensor.Status);
                Console.WriteLine($"{sensor.Id}\t{sensorType}\t{statusText}\t\t{sensor.CurrentValue:F2}");
                Console.ResetColor();
            }
        }

        private static async Task UpdateSensorMenuAsync(SensorSimulationJob sensorJob)
        {
            // Önce sensörleri göster
            DisplaySensors(sensorJob);

            // Sensör listesini al
            var sensors = sensorJob.GetSensors();

            if (sensors == null || !sensors.Any())
            {
                return;
            }

            Console.Write("\nGüncellemek istediğiniz sensörün ID'sini girin: ");
            if (!int.TryParse(Console.ReadLine(), out int sensorId))
            {
                Console.WriteLine("Geçersiz ID!");
                return;
            }

            var sensor = sensors.FirstOrDefault(s => s.Id == sensorId);
            if (sensor == null)
            {
                Console.WriteLine("Bu ID'ye sahip sensör bulunamadı!");
                return;
            }

            Console.WriteLine($"\nSeçilen sensör: {sensor.Id}, Tip: {sensor.Type}, Mevcut değer: {sensor.CurrentValue:F2}");
            Console.Write("Yeni değer girin (veya rastgele değer için 'r' yazın): ");

            string input = Console.ReadLine();
            double? newValue = null;

            if (input.ToLower() != "r")
            {
                if (double.TryParse(input, out double value))
                {
                    newValue = value;
                }
                else
                {
                    Console.WriteLine("Geçersiz değer! Rastgele değer atanacak.");
                }
            }

            double oldValue = sensor.CurrentValue;
            var oldStatus = sensor.Status;

            await sensorJob.UpdateAndSendSensorAsync(sensor, newValue);

            Console.WriteLine($"Sensör güncellendi:");
            Console.WriteLine($"Değer: {oldValue:F2} -> {sensor.CurrentValue:F2}");
            Console.WriteLine($"Durum: {GetStatusDisplayText(oldStatus)} -> {GetStatusDisplayText(sensor.Status)}");
        }

        // Sensör durumuna göre Türkçe metin döndür
        private static string GetStatusDisplayText(SensorStatus status)
        {
            return status switch
            {
                SensorStatus.Nominal => "Normal",
                SensorStatus.Warning => "Uyarı",
                SensorStatus.Critical => "Kritik",
                SensorStatus.Offline => "Kapalı",
                _ => "Tanımsız"
            };
        }

        // Sensör durumuna göre renk adı döndür
        private static string GetStatusColor(SensorStatus status)
        {
            return status switch
            {
                SensorStatus.Nominal => "Yeşil",
                SensorStatus.Warning => "Sarı",
                SensorStatus.Critical => "Kırmızı",
                _ => "Gri"
            };
        }

        // Console renkleri için yardımcı metot
        private static ConsoleColor GetConsoleColorForStatus(SensorStatus status)
        {
            return status switch
            {
                SensorStatus.Nominal => ConsoleColor.Green,
                SensorStatus.Warning => ConsoleColor.Yellow,
                SensorStatus.Critical => ConsoleColor.Red,
                _ => ConsoleColor.Gray
            };
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Logger ekle
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Servisleri kaydet
            services.AddSingleton<ISensorRangeService, SensorRangeService>();
            services.AddSingleton<ISensorSimulatorService, SensorSimulatorService>();
            services.AddSingleton<INetworkService, NetworkService>();
            services.AddSingleton<SensorDataGenerator>();
            services.AddSingleton<Random>();
            // SensorSimulationJob'ı kaydet
            services.AddSingleton<SensorSimulationJob>();
            services.AddSingleton<ISensorSimulationJob, SensorSimulationJob>();

            return services.BuildServiceProvider();
        }
    }
}
