using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GroundSystems.Server.Models.Enums;
using Microsoft.Extensions.Logging;

namespace GroundSystems.Server.Services
{
    public class NetworkService : INetworkService
    {
        private TcpListener _server;
        private readonly List<TcpClient> _clients = new List<TcpClient>();
        private readonly object _lock = new object();
        private bool _isRunning;
        private readonly int _port = 5000;
        public NetworkService()
        {
           
        }
        public bool IsRunning => _isRunning;
        public event EventHandler<string> DataReceived;

        public async Task StartServer()
        {
            try
            {
                _server = new TcpListener(IPAddress.Any, _port);
                _server.Start();
                _isRunning = true;

                await AcceptClientsAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task AcceptClientsAsync()
        {
            while (_isRunning)
            {
                try
                {
                    var client = await _server.AcceptTcpClientAsync();
                    lock (_lock)
                    {
                        _clients.Add(client);
                    }
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex) when (_isRunning)
                {
                }
            }
        }


        private async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] lengthBuffer = new byte[4];

                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4);
                    if (bytesRead < 4) break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0 || messageLength > 1024 * 1024)
                    {
                        continue;
                    }

                    byte[] messageBuffer = new byte[messageLength];
                    bytesRead = await stream.ReadAsync(messageBuffer, 0, messageLength);
                    if (bytesRead < messageLength) break;

                    string jsonData = Encoding.UTF8.GetString(messageBuffer);

                    // Gelen veriyi ilgili event aracılığıyla bildir
                    DataReceived?.Invoke(this, jsonData);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                stream?.Dispose();

                if (client != null)
                {
                    lock (_lock)
                    {
                        _clients.Remove(client);
                    }
                    client.Dispose();
                }
            }
        }

        public async Task StopServer()
        {
            _isRunning = false;

            foreach (var client in _clients.ToList())
            {
                client.Close();
            }

            _server?.Stop();
        }
    }
}
