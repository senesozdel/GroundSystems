using GroundSystems.Server.Models.Entities;
using GroundSystems.Server.Models.Enums;
using GroundSystems.Server.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GroundSystems.Server
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(
        ICustomLogService logService,
        INetworkService networkService,
        ISensorDataProcessor sensorProcessor,
        ISensorService sensorService)
        {
            InitializeComponent();

            _viewModel = new MainViewModel(logService, networkService, sensorProcessor, sensorService);
            DataContext = _viewModel;

            Loaded += async (s, e) => await _viewModel.InitializeAsync();
            Closed += async (s, e) => await _viewModel.CleanupAsync();
        }

    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ICustomLogService _logService;
        private readonly INetworkService _networkService;
        private readonly ISensorDataProcessor _sensorProcessor;
        private readonly ISensorService _sensorService;
        private System.Windows.Threading.DispatcherTimer _connectionCheckTimer;
        private string _logText;
        public string LogText
        {
            get => _logText;
            set
            {
                _logText = value;
                OnPropertyChanged(nameof(LogText));
            }
        }
        public ObservableCollection<Sensor> Sensors { get; } = new ObservableCollection<Sensor>();
        public ObservableCollection<string> LogMessages { get; } = new ObservableCollection<string>();

        public MainViewModel(
            ICustomLogService logService,
            INetworkService networkService,
            ISensorDataProcessor sensorProcessor,
            ISensorService sensorService)
        {
            _logService = logService;
            _networkService = networkService;
            _sensorProcessor = sensorProcessor;
            _sensorService = sensorService;

            // Sensör güncellendi event'ini dinle
            _sensorProcessor.SensorUpdated += SensorProcessor_SensorUpdated;

            // Ağ servisinden DataReceived olayını dinle
            _networkService.DataReceived += NetworkService_DataReceived;
        }

        public async Task InitializeAsync()
        {
            await _logService.LogApplicationEventAsync("MainViewModel", "Application started", LogLevel.Information);
            AddLogMessage("Application started");

            // Veritabanından sensörleri yükle
            await LoadSensorsFromDatabaseAsync();

            // Ağ dinlemeyi başlat
            await StartNetworkListeningAsync();

            InitializeConnectionCheckTimer();
        }

        private void InitializeConnectionCheckTimer()
        {
            _connectionCheckTimer = new System.Windows.Threading.DispatcherTimer();
            _connectionCheckTimer.Tick += ConnectionCheckTimer_Tick;
            _connectionCheckTimer.Interval = TimeSpan.FromSeconds(1); // Her saniye kontrol et
            _connectionCheckTimer.Start();

            AddLogMessage("Sensör bağlantı kontrol mekanizması başlatıldı");
        }

        private async void ConnectionCheckTimer_Tick(object sender, EventArgs e)
        {
            var currentTime = DateTime.Now;
            bool anyStatusChanged = false;

            foreach (var sensor in Sensors)
            {
                // Eğer son güncellenme zamanından bu yana 5 saniyeden fazla geçtiyse
                // ve sensör zaten Offline değilse, durumu Offline olarak güncelle
                if ((currentTime - sensor.Timestamp).TotalSeconds > 5 && sensor.Status != SensorStatus.Offline)
                {
                    sensor.Status = SensorStatus.Offline;
                    anyStatusChanged = true;

                    // Log mesajı oluştur
                    string message = $"5 saniye veri gelmedi, durumu İnaktif olarak değiştirildi";

                    // UI loguna ekle
                    AddLogMessage($"[{DateTime.Now:HH:mm:ss}] Sensör {sensor.Id} - {message}");

                    // Sensörün kendi log dosyasına yaz
                    await _logService.LogSensorEventAsync(
                        sensor.Id,
                        message,
                        LogLevel.Warning);

                    // Sensörü veritabanında güncelle
                    await _sensorService.UpdateSensorAsync(sensor);
                }
            }

            // Herhangi bir sensörün durumu değiştiyse Property'yi güncelle
            if (anyStatusChanged)
            {
                OnPropertyChanged(nameof(Sensors));
            }
        }

        private async Task LoadSensorsFromDatabaseAsync()
        {
            try
            {
                AddLogMessage("Loading sensors from database...");

                var dbSensors = await _sensorService.GetAllSensorsAsync();

                foreach (var sensor in dbSensors)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        Sensors.Add(sensor);
                    });

                    // Her sensör için yükleme logu 
                    await _logService.LogSensorEventAsync(
                        sensor.Id,
                        $"Sensör {(sensor.Name != null ? sensor.Name : "İsimsiz")} yüklendi - Başlangıç durumu: {GetStatusText(sensor.Status)}",
                        LogLevel.Information);
                }

                AddLogMessage($"Loaded {dbSensors.Count()} sensors");
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(ex, "Error loading sensors");
                AddLogMessage($"ERROR: {ex.Message}");
            }
        }

        private async Task StartNetworkListeningAsync()
        {
            try
            {
                AddLogMessage("Starting network listening...");
                await _networkService.StartServer();
                AddLogMessage("Network listening started, waiting for sensor data...");
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(ex, "Error starting network listener");
                AddLogMessage($"ERROR: {ex.Message}");
            }
        }

        private void NetworkService_DataReceived(object sender, string jsonData)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                AddLogMessage($"[{DateTime.Now:HH:mm:ss}] Data received");
                await _sensorProcessor.ProcessSensorDataAsync(jsonData);
            });
        }

        private async void SensorProcessor_SensorUpdated(object sender, Sensor updatedSensor)
        {
            if (updatedSensor == null)
                return;

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    // Sensör koleksiyonunda mevcut sensörü bul
                    var existingSensor = Sensors.FirstOrDefault(s => s.Id == updatedSensor.Id);
                    bool isNew = existingSensor == null;
                    bool statusChanged = existingSensor != null && existingSensor.Status != updatedSensor.Status;

                    // Sensör koleksiyonunu güncelle
                    if (isNew)
                    {
                        Sensors.Add(updatedSensor);
                    }
                    else
                    {
                        int index = Sensors.IndexOf(existingSensor);
                        Sensors[index] = updatedSensor;
                    }

                    // Log işlemleri için bilgileri hazırla
                    string statusText = GetStatusText(updatedSensor.Status);
                    LogLevel logLevel = DetermineLogLevelForStatus(updatedSensor.Status);

                    // UI log mesajı ekle
                    string uiLogMessage = isNew
                        ? $"Yeni Sensör {updatedSensor.Id} ({updatedSensor.Name}) - Durum: {statusText}"
                        : $"Sensör {updatedSensor.Id} ({updatedSensor.Name}) - Durum: {statusText}";

                    AddLogMessage($"[{DateTime.Now:HH:mm:ss}] {uiLogMessage}");

                    // Sensör loglama işlemleri
                    if (isNew)
                    {
                        // Yeni sensör eklendi
                        await _logService.LogSensorEventAsync(
                            updatedSensor.Id,
                            $"Yeni sensör tespit edildi - Durum: {statusText}",
                            LogLevel.Information);
                    }
                    else if (statusChanged)
                    {
                        // Sensör durumu değişti
                        await _logService.LogSensorEventAsync(
                            updatedSensor.Id,
                            $"Sensör durumu değişti: {GetStatusText(existingSensor.Status)} -> {statusText}",
                            logLevel);
                    }

                    // Sensör veri güncellemesi
                    await _logService.LogSensorEventAsync(
                        updatedSensor.Id,
                        $"Veri güncellendi - '{updatedSensor.Name}' sensörü şu değere sahip: {updatedSensor.CurrentValue}",
                        LogLevel.Information);
                }
                catch (Exception ex)
                {
                    AddLogMessage($"[{DateTime.Now:HH:mm:ss}] HATA: Sensör güncellemesi sırasında hata oluştu: {ex.Message}");
                    await _logService.LogSensorEventAsync(
                        updatedSensor.Id ,
                        $"Sensör güncellemesi hatası: {ex.Message}",
                        LogLevel.Error);
                }
            });
        }

        // Sensör durumuna göre uygun log seviyesini belirle
        private LogLevel DetermineLogLevelForStatus(SensorStatus status)
        {
            switch (status)
            {
                case SensorStatus.Nominal:
                    return LogLevel.Information;
                case SensorStatus.Warning:
                    return LogLevel.Warning;
                case SensorStatus.Critical:
                    return LogLevel.Error;
                case SensorStatus.Offline:
                    return LogLevel.Warning;
                default:
                    return LogLevel.Information;
            }
        }

        private string GetStatusText(SensorStatus status)
        {
            switch (status)
            {
                case SensorStatus.Nominal:
                    return "İyi";
                case SensorStatus.Warning:
                    return "Uyarı";
                case SensorStatus.Critical:
                    return "Kritik";
                case SensorStatus.Offline:
                    return "İnaktif";
                default:
                    return "Bilinmiyor";
            }
        }

        private void AddLogMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                LogMessages.Add(message);
                OnPropertyChanged(nameof(LogMessages));

                LogText = string.Join(Environment.NewLine, LogMessages);
            });
        }

        public async Task CleanupAsync()
        {
            _connectionCheckTimer?.Stop();
            // Olayları temizle
            _sensorProcessor.SensorUpdated -= SensorProcessor_SensorUpdated;
            _networkService.DataReceived -= NetworkService_DataReceived;

            // Tüm aktif sensörler için kapanış logu
            foreach (var sensor in Sensors)
            {
                await _logService.LogSensorEventAsync(
                    sensor.Id,
                    "Uygulama kapatıldı, sensör takibi durduruldu",
                    LogLevel.Information);
            }

            // Sunucuyu durdur
            await _networkService.StopServer();

            // Kapatma logunu yaz
            await _logService.LogApplicationEventAsync("MainViewModel", "Application closed", LogLevel.Information);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
