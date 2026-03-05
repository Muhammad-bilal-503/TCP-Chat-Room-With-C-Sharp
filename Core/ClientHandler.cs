using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ServerChat.Services;

namespace ServerChat.Core
{
    public class ClientHandler
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private AuthService _authService;
        private DatabaseService _db;        // ✅ NEW
        private Server _server;
        private string _username;

        public ClientHandler(TcpClient client, Server server)
        {
            _client = client;
            _stream = client.GetStream();
            _authService = new AuthService();
            _db = new DatabaseService();  // ✅ NEW
            _server = server;
        }

        public async Task HandleAsync()
        {
            byte[] buffer = new byte[8192];

            try
            {
                // ✅ Pehle AUTH_REQUIRED bhejo
                await SendPacket("AUTH_REQUIRED");

                int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
                string encryptedPacket = Encoding.UTF8.GetString(buffer, 0, bytes).Trim();

                // ✅ Decrypt karo
                string firstPacket = EncryptionService.Decrypt(encryptedPacket);

                var parts = firstPacket.Split('|');

                // ================= REGISTER =================
                if (parts[0] == "REGISTER" && parts.Length == 3)
                {
                    string username = parts[1].Trim();
                    string password = parts[2].Trim();

                    string regResult = _authService.Register(username, password);
                    await SendPacket(regResult);
                    _client.Close();
                    return;
                }

                // ================= LOGIN_CHECK — LoginForm validation =================
                else if (parts[0] == "LOGIN_CHECK" && parts.Length == 3)
                {
                    string username = parts[1].Trim();
                    string password = parts[2].Trim();

                    string loginResult = _authService.Login(username, password);
                    await SendPacket(loginResult);
                    _client.Close();
                    return;
                }

                // ================= LOGIN — Chat Session =================
                else if (parts[0] == "LOGIN" && parts.Length == 3)
                {
                    string username = parts[1].Trim();
                    string password = parts[2].Trim();

                    string loginResult = _authService.Login(username, password);

                    if (loginResult != "LOGIN_SUCCESS")
                    {
                        await SendPacket("AUTH_FAILED");
                        _client.Close();
                        return;
                    }

                    if (_server.IsUserConnected(username))
                    {
                        await SendPacket("DUPLICATE_USER");
                        _client.Close();
                        return;
                    }

                    _username = username;
                    _server.AddClient(_client, _username);

                    await SendPacket("AUTH_SUCCESS");
                    _server.Broadcast($"{_username} joined the chat.", _client);

                    // ✅ Chat history bhejo
                    var history = _server.GetChatHistory();
                    foreach (var item in history)
                    {
                        // ✅ Encrypt karke bhejo
                        string historyMsg = EncryptionService.Encrypt(
                            $"HISTORY:{item.Sender}:{item.Message}:{item.Time}");
                        await SendPacket($"MSG:{historyMsg}");
                        // ✅ Thoda delay taake packets mix na hon
                        await Task.Delay(10);
                    }

                    // ================= MESSAGE LOOP =================
                    while (true)
                    {
                        bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytes == 0) break;

                        string msg = Encoding.UTF8.GetString(buffer, 0, bytes);

                        if (msg.StartsWith("MSG:"))
                        {
                            string encryptedMsg = msg.Substring(4);
                            //Decrypt karo
                            string cleanMsg = EncryptionService.Decrypt(encryptedMsg);

                            // ✅ Database mein save karo
                            _db.SaveMessage(_username, cleanMsg);

                            _server.Broadcast($"{_username}: {cleanMsg}", _client);
                        }
                        else if (msg.StartsWith("FILE:"))
                        {
                            _server.Log($"File received from {_username}");
                        }
                    }
                }
                else
                {
                    await SendPacket("AUTH_FAILED");
                    _client.Close();
                }
            }
            catch { }
            finally
            {
                _server.RemoveClient(_client);
                _client.Close();
            }
        }

        private async Task SendPacket(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(data, 0, data.Length);
        }
    }
}