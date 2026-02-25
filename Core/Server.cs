using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerChat.Core
{
    public class Server
    {
        private TcpListener _listener;

        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();

            while (true)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();

                ClientHandler handler = new ClientHandler(client);
                _ = handler.HandleAsync();
            }
        }
    }
}