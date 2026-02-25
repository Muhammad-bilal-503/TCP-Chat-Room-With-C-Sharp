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
        private AuthService _authService = new AuthService();

        public ClientHandler(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public async Task HandleAsync()
        {
            byte[] buffer = new byte[1024];

            int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.ASCII.GetString(buffer, 0, bytes);

            string[] parts = message.Split('|');

            if (parts.Length < 3)
                return;

            string command = parts[0];
            string username = parts[1];
            string password = parts[2];

            string response = "";

            if (command == "REGISTER")
            {
                bool result = _authService.Register(username, password);
                response = result ? "REGISTER_SUCCESS" : "USER_EXISTS";
            }
            else if (command == "LOGIN")
            {
                bool result = _authService.Login(username, password);
                response = result ? "LOGIN_SUCCESS" : "LOGIN_FAILED";
            }

            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            await _stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}