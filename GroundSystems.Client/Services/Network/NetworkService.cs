using GroundSystems.Client.Models;
using GroundSystems.Server.Models.RequestModels;
using System.Net.Sockets;
using System.Text;
using GroundSystems.Server.Models.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GroundSystems.Server.Models.Entities;

namespace GroundSystems.Client.Services.Network
{
    public class NetworkService : INetworkService, IDisposable
    {
        private readonly string _serverIp;
        private readonly int _serverPort;
        private TcpClient _client;
        private NetworkStream _stream;
        private bool _isConnected;

        public NetworkService(string serverIp = "127.0.0.1", int serverPort = 5000)
        {
            _serverIp = serverIp;
            _serverPort = serverPort;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(_serverIp, _serverPort);
                _stream = _client.GetStream();
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> SendSensorDataAsync(Sensor sensorData)
        {
            if (!_isConnected)
            {
                try
                {
                    await ConnectAsync();
                }
                catch
                {
                    return false;
                }
            }

            try
            {


                // JSON serialize
                string jsonData = JsonSerializer.Serialize(sensorData);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);

                // İlk olarak mesaj uzunluğunu gönder (4 byte)
                byte[] lengthPrefix = BitConverter.GetBytes(dataBytes.Length);
                await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);

                // Ardından mesajın kendisini gönder
                await _stream.WriteAsync(dataBytes, 0, dataBytes.Length);
                await _stream.FlushAsync();

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                return false;
            }
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}