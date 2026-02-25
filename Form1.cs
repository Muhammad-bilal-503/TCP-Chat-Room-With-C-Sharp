// FULL PROFESSIONAL A+ TCP CHAT SERVER
// Muhammad Bilal - Advanced Version

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ServerChat
{
    public partial class Form1 : Form
    {
        TcpListener server;
        Dictionary<TcpClient, string> clients = new Dictionary<TcpClient, string>();
        bool serverRunning = false;

        string host = "127.0.0.1";
        int port = 55555;
        string serverPassword = "123";
        int maxClients = 20;

        // UI
        Button btnStart, btnStop, btnFile, btnTheme, btnDisconnect, btnPrivate;
        Label lblStatus, lblCount;
        RichTextBox txtLogs;
        ListBox lstClients;
        TextBox txtBroadcast, txtPrivate;

        bool darkMode = true;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        void SetupUI()
        {
            this.Text = "A+ Professional TCP Chat Server";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnStart = new Button() { Text = "Start", Location = new Point(20, 20), Width = 80 };
            btnStop = new Button() { Text = "Stop", Location = new Point(110, 20), Width = 80 };
            btnFile = new Button() { Text = "Send File", Location = new Point(200, 20), Width = 100 };
            btnTheme = new Button() { Text = "Toggle Theme", Location = new Point(310, 20), Width = 120 };

            lblStatus = new Label() { Text = "● Stopped", Location = new Point(450, 25), AutoSize = true };
            lblCount = new Label() { Text = "Clients: 0", Location = new Point(550, 25), AutoSize = true };

            txtLogs = new RichTextBox() { Location = new Point(20, 60), Size = new Size(600, 500), ReadOnly = true };
            lstClients = new ListBox() { Location = new Point(650, 60), Size = new Size(300, 250) };

            btnDisconnect = new Button() { Text = "Disconnect", Location = new Point(650, 320), Width = 140 };
            txtPrivate = new TextBox() { Location = new Point(650, 360), Width = 300 };
            btnPrivate = new Button() { Text = "Send Private", Location = new Point(650, 390), Width = 140 };

            txtBroadcast = new TextBox() { Location = new Point(20, 580), Width = 500 };
            Button btnBroadcast = new Button() { Text = "Broadcast", Location = new Point(530, 580), Width = 90 };

            this.Controls.AddRange(new Control[] {
                btnStart, btnStop, btnFile, btnTheme,
                lblStatus, lblCount, txtLogs, lstClients,
                btnDisconnect, txtPrivate, btnPrivate,
                txtBroadcast, btnBroadcast
            });

            btnStart.Click += async (s, e) => await StartServer();
            btnStop.Click += StopServer;
            btnFile.Click += SendFile;
            btnTheme.Click += ToggleTheme;
            btnDisconnect.Click += DisconnectClient;
            btnPrivate.Click += SendPrivate;
            btnBroadcast.Click += (s, e) => BroadcastMessage();

            ApplyTheme();
        }

        async Task StartServer()
        {
            if (serverRunning) return;

            server = new TcpListener(IPAddress.Parse(host), port);
            server.Start();
            serverRunning = true;

            lblStatus.Text = "● Running";
            lblStatus.ForeColor = Color.Green;

            Log("Server Started...");

            while (serverRunning)
            {
                var client = await server.AcceptTcpClientAsync();

                if (clients.Count >= maxClients)
                {
                    client.Close();
                    continue;
                }

                _ = HandleClient(client);
            }
        }

        void StopServer(object sender, EventArgs e)
        {
            serverRunning = false;
            server?.Stop();

            foreach (var client in clients.Keys)
                client.Close();

            clients.Clear();
            lstClients.Items.Clear();
            UpdateCount();

            lblStatus.Text = "● Stopped";
            lblStatus.ForeColor = Color.Red;

            Log("Server Stopped.");
        }

        async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();

            byte[] nickRequest = Encoding.ASCII.GetBytes("NICK");
            await stream.WriteAsync(nickRequest, 0, nickRequest.Length);

            byte[] buffer = new byte[1024];
            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            string nickname = Encoding.ASCII.GetString(buffer, 0, bytes);

            clients.Add(client, nickname);

            Invoke(new Action(() =>
            {
                lstClients.Items.Add(nickname);
                UpdateCount();
            }));

            Broadcast($"{nickname} joined.");

            try
            {
                while (true)
                {
                    bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytes == 0) break;

                    string msg = Encoding.ASCII.GetString(buffer, 0, bytes);
                    Broadcast(msg, client);
                }
            }
            catch { }

            RemoveClient(client);
        }

        void Broadcast(string message, TcpClient sender = null)
        {
            byte[] data = Encoding.ASCII.GetBytes($"[{DateTime.Now:T}] {message}");
            foreach (var client in clients.Keys)
            {
                if (client != sender)
                {
                    try { client.GetStream().Write(data, 0, data.Length); }
                    catch { }
                }
            }
            Log(message);
        }

        void BroadcastMessage()
        {
            if (string.IsNullOrWhiteSpace(txtBroadcast.Text)) return;
            Broadcast("Server: " + txtBroadcast.Text);
            txtBroadcast.Clear();
        }

        void SendPrivate(object sender, EventArgs e)
        {
            if (lstClients.SelectedItem == null) return;

            string selected = lstClients.SelectedItem.ToString();
            var client = GetClientByName(selected);

            if (client != null)
            {
                byte[] data = Encoding.ASCII.GetBytes("Private from Server: " + txtPrivate.Text);
                client.GetStream().Write(data, 0, data.Length);
                Log($"Private sent to {selected}");
                txtPrivate.Clear();
            }
        }

        TcpClient GetClientByName(string name)
        {
            foreach (var pair in clients)
                if (pair.Value == name) return pair.Key;
            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void DisconnectClient(object sender, EventArgs e)
        {
            if (lstClients.SelectedItem == null) return;

            string pass = Microsoft.VisualBasic.Interaction.InputBox("Enter Password:", "Security");
            if (pass != serverPassword)
            {
                MessageBox.Show("Wrong Password!");
                return;
            }

            var client = GetClientByName(lstClients.SelectedItem.ToString());
            RemoveClient(client);
        }

        void RemoveClient(TcpClient client)
        {
            if (client == null) return;

            string name = clients[client];
            clients.Remove(client);
            client.Close();

            Invoke(new Action(() =>
            {
                lstClients.Items.Remove(name);
                UpdateCount();
            }));

            Broadcast($"{name} left.");
        }

        void SendFile(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            byte[] file = File.ReadAllBytes(ofd.FileName);
            string header = $"FILE:{Path.GetFileName(ofd.FileName)}:{file.Length}";
            Broadcast(header);

            foreach (var client in clients.Keys)
            {
                try { client.GetStream().Write(file, 0, file.Length); }
                catch { }
            }

            Log("File Sent.");
        }

        void UpdateCount()
        {
            lblCount.Text = "Clients: " + clients.Count;
        }

        void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(message)));
                return;
            }

            txtLogs.AppendText($"[{DateTime.Now:T}] {message}\n");
            txtLogs.ScrollToCaret();
            File.AppendAllText("server_logs.txt", $"[{DateTime.Now}] {message}\n");
        }

        void ToggleTheme(object sender, EventArgs e)
        {
            darkMode = !darkMode;
            ApplyTheme();
        }

        void ApplyTheme()
        {
            if (darkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                txtLogs.BackColor = Color.Black;
                txtLogs.ForeColor = Color.Lime;
            }
            else
            {
                this.BackColor = Color.White;
                txtLogs.BackColor = Color.White;
                txtLogs.ForeColor = Color.Black;
            }
        }
    }
}