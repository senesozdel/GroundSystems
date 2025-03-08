using GroundSystems.Server.Context;
using GroundSystems.Server.Repositories;
using GroundSystems.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace GroundSystems.Server
{
    /// <summary>
    /// App.xaml etkileşim mantığı
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();

            // Loglama servislerini ekle
            services.AddLogging(configure =>
            {
                configure.AddConsole();
            });

            // AppDbContext için factory ekleyelim
            services.AddTransient<AppDbContext>(provider =>
            {
                // EF6 için DbContext oluşturma - Connection string'i buraya ekleyin
                // Ya connection string adını verin
                return new AppDbContext();

                // VEYA direkt connection string'i verin
                // return new AppDbContext("Data Source=YourServer;Initial Catalog=YourDB;Integrated Security=True");
            });

            // Repository ve diğer servisler
            services.AddSingleton<ISensorRepository, SensorRepository>();
            services.AddSingleton<ICustomLogService, CustomLogService>();
            services.AddSingleton<ISensorService, SensorService>();
            services.AddSingleton<ISensorDataProcessor, SensorDataProcessor>();
            services.AddSingleton<INetworkService, NetworkService>();

            // MainWindow
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }



        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<AppDbContext>(provider => new AppDbContext());
            services.AddSingleton<ICustomLogService, CustomLogService>();
            services.AddSingleton<INetworkService, NetworkService>();
            services.AddSingleton<ISensorDataProcessor, SensorDataProcessor>();
            services.AddScoped<ISensorService, SensorService>();
            services.AddScoped<ISensorRepository, SensorRepository>();

            services.AddSingleton<ICustomLogService, CustomLogService>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }


    }
}
