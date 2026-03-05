using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerChat.Core
{
    public class Server
    {
        private TcpListener _listener;
        private Dictionary<TcpClient, string> _clients = new Dictionary<TcpClient, string>();
        private int _maxClients = 20;

        // ✅ Form1 ko logs bhejne ke liye
        public event Action<string> OnLog;
        public event Action<string> OnClientConnected;
        public event Action<string> OnClientDisconnected;
        public event Action<int> OnClientCountChanged;

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            OnLog?.Invoke("Server Started...");

            while (true)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();

                    if (_clients.Count >= _maxClients)
                    {
                        client.Close();
                        OnLog?.Invoke("Max clients reached. Connection rejected.");
                        continue;
                    }

                    // ✅ ClientHandler ko server ka reference dete hain
                    ClientHandler handler = new ClientHandler(client, this);
                    _ = handler.HandleAsync();
                }
                catch (Exception ex)
                {
                    OnLog?.Invoke("Accept Error: " + ex.Message);
                    break;
                }
            }
        }

        public void Stop()
        {
            try
            {
                _listener.Stop();
                OnLog?.Invoke("Server Stopped.");
            }
            catch (Exception ex)
            {
                OnLog?.Invoke("Stop Error: " + ex.Message);
            }
        }

        // ✅ Client add karo — ClientHandler call karega
        public void AddClient(TcpClient client, string username)
        {
            lock (_clients)
            {
                _clients[client] = username;
            }

            OnClientConnected?.Invoke(username);
            OnClientCountChanged?.Invoke(_clients.Count);
            OnLog?.Invoke($"{username} connected.");
        }

        // ✅ Client remove karo
        public void RemoveClient(TcpClient client)
        {
            string username = "";

            lock (_clients)
            {
                if (_clients.ContainsKey(client))
                {
                    username = _clients[client];
                    _clients.Remove(client);
                }
            }

            if (!string.IsNullOrEmpty(username))
            {
                OnClientDisconnected?.Invoke(username);
                OnClientCountChanged?.Invoke(_clients.Count);
                OnLog?.Invoke($"{username} disconnected.");
                Broadcast($"{username} left the chat.", null);
            }
        }

        // ✅ Sab clients ko message bhejo
        public void Broadcast(string message, TcpClient sender)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes("MSG:" + message);

            lock (_clients)
            {
                foreach (var client in _clients.Keys)
                {
                    if (client != sender)
                    {
                        try
                        {
                            client.GetStream().Write(data, 0, data.Length);
                        }
                        catch { }
                    }
                }
            }

            OnLog?.Invoke(message);
        }

        // ✅ Kisi ek client ko private message bhejo
        public void SendPrivate(string toUsername, string message)
        {
            lock (_clients)
            {
                foreach (var pair in _clients)
                {
                    if (pair.Value == toUsername)
                    {
                        // ✅ [Private] hata diya — seedha message bhejo
                        byte[] data = System.Text.Encoding.UTF8.GetBytes("MSG:" + message);
                        try
                        {
                            pair.Key.GetStream().Write(data, 0, data.Length);
                        }
                        catch { }
                        break;
                    }
                }
            }
        }

        // ✅ Username already connected hai?
        public bool IsUserConnected(string username)
        {
            lock (_clients)
            {
                return _clients.ContainsValue(username);
            }
        }

        // ✅ Connected clients ki list
        public List<string> GetConnectedUsers()
        {
            lock (_clients)
            {
                return new List<string>(_clients.Values);
            }
        }

        // ✅ Yeh method ClientHandler use karega
        public void Log(string message)
        {
            OnLog?.Invoke(message);
        }

        // ✅ Recent messages load karo
        public List<(string Sender, string Message, string Time)> GetChatHistory(string username)
        {
            var db = new ServerChat.Services.DatabaseService();
            // ✅ Sirf us user ki history
            return db.GetUserMessages(username);
        }

        // ✅ Typing indicator broadcast
        public void BroadcastTyping(string encryptedData, TcpClient sender)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(
                "TYPING:" + encryptedData);

            lock (_clients)
            {
                foreach (var client in _clients.Keys)
                {
                    if (client != sender)
                    {
                        try
                        {
                            client.GetStream().Write(data, 0, data.Length);
                        }
                        catch { }
                    }
                }
            }
        }

        // ✅ Username se client forcefully disconnect karo
        public void ForceDisconnect(string username)
        {
            TcpClient targetClient = null;

            lock (_clients)
            {
                foreach (var pair in _clients)
                {
                    if (pair.Value == username)
                    {
                        targetClient = pair.Key;
                        break;
                    }
                }
            }

            if (targetClient != null)
            {
                RemoveClient(targetClient);
                targetClient.Close();
            }
        }
    }
}