using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ServerChat.Core;

namespace ServerChat
{
    public partial class Form1 : Form
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll",
        EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect,
            int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        private Server _server;
        private bool _serverRunning = false;
        private int _port = 55555;

        // UI
        Button btnStart, btnStop, btnFile, btnTheme, btnDisconnect, btnPrivate;
        Label lblStatus, lblCount;
        Panel chatScroll, chatPanel;
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
            this.Text = "Professional TCP Chat Server";
            this.Size = new Size(1000, 670);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnStart = new Button() { Text = "Start", Location = new Point(20, 20), Width = 80, ForeColor = Color.White };
            btnStop = new Button() { Text = "Stop", Location = new Point(110, 20), Width = 80, ForeColor = Color.White };
            btnFile = new Button() { Text = "Send File", Location = new Point(200, 20), Width = 100 , ForeColor = Color.White };
            btnTheme = new Button() { Text = "Toggle Theme", Location = new Point(310, 20), Width = 120 , ForeColor = Color.White };

            lblStatus = new Label() { Text = "● Stopped", Location = new Point(450, 25), AutoSize = true, ForeColor = Color.Red };
            lblCount = new Label()
            {
                Text = "Clients: 0",
                Location = new Point(550, 25),
                AutoSize = true,
                // ✅ Default dark theme ke liye white
                ForeColor = Color.White
            };

            // ✅ WhatsApp style chat area
            chatScroll = new Panel()
            {
                Location = new Point(20, 60),
                Size = new Size(600, 520),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(20, 20, 20)
            };

            chatPanel = new Panel()
            {
                Width = 580,
                Height = 0,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            chatScroll.Controls.Add(chatPanel);

            // ✅ Clients list
            lstClients = new ListBox()
            {
                Location = new Point(650, 60),
                Size = new Size(300, 250),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };

            btnDisconnect = new Button() { Text = "Disconnect Client", Location = new Point(650, 320), Width = 160 };

            txtPrivate = new TextBox() { Location = new Point(650, 360), Width = 300 };
            btnPrivate = new Button() { Text = "Send Private", Location = new Point(650, 390), Width = 140 };

            txtBroadcast = new TextBox() { Location = new Point(20, 600), Width = 500 };
            Button btnBroadcast = new Button() { Text = "Broadcast", Location = new Point(530, 600), Width = 90 };

            this.Controls.AddRange(new Control[] {
                btnStart, btnStop, btnFile, btnTheme,
                lblStatus, lblCount,
                chatScroll,
                lstClients,
                btnDisconnect, txtPrivate, btnPrivate,
                txtBroadcast, btnBroadcast
            });

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;
            btnFile.Click += BtnFile_Click;
            btnTheme.Click += ToggleTheme;
            btnDisconnect.Click += BtnDisconnect_Click;
            btnPrivate.Click += BtnPrivate_Click;
            btnBroadcast.Click += (s, e) => BroadcastMessage();

            ApplyTheme();
        }

        // ================= SERVER START =================

        async void BtnStart_Click(object sender, EventArgs e)
        {
            if (_serverRunning) return;

            _server = new Server(_port);

            _server.OnLog += Log;
            _server.OnClientConnected += username => AddClientToList(username);
            _server.OnClientDisconnected += username => RemoveClientFromList(username);
            _server.OnClientCountChanged += count => UpdateCount(count);

            _serverRunning = true;

            lblStatus.Text = "● Running";
            lblStatus.ForeColor = Color.Green;

            AppendSystemMessage("Server started on port " + _port);

            await _server.StartAsync();
        }

        // ================= SERVER STOP =================

        void BtnStop_Click(object sender, EventArgs e)
        {
            if (!_serverRunning) return;

            _server.Stop();
            _serverRunning = false;

            lblStatus.Text = "● Stopped";
            lblStatus.ForeColor = Color.Red;

            lstClients.Items.Clear();
            UpdateCount(0);

            AppendSystemMessage("Server stopped.");
        }

        // ================= BROADCAST =================

        void BroadcastMessage()
        {
            if (!_serverRunning) return;
            if (string.IsNullOrWhiteSpace(txtBroadcast.Text)) return;

            string msg = txtBroadcast.Text.Trim();
            _server.Broadcast("Server: " + msg, null);

            // ✅ Server ka apna message right side par
            AppendMessage("Server (You)", msg, isMe: true);

            txtBroadcast.Clear();
        }

        // ================= PRIVATE MESSAGE =================

        void BtnPrivate_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItem == null) return;
            if (string.IsNullOrWhiteSpace(txtPrivate.Text)) return;

            string selectedUser = lstClients.SelectedItem.ToString();
            string msg = txtPrivate.Text.Trim();

            _server.SendPrivate(selectedUser, "Server (Private Message): " + msg);

            AppendSystemMessage($"Private → {selectedUser}: {msg}");
            txtPrivate.Clear();
        }

        // ================= DISCONNECT CLIENT =================

        void BtnDisconnect_Click(object sender, EventArgs e)
        {
            if (lstClients.SelectedItem == null)
            {
                AppendSystemMessage("Select the Client First.");
                return;
            }

            // ✅ Password check
            string pass = Microsoft.VisualBasic.Interaction.InputBox(
                "If You Disconnect the Client Enter Password:",
                "Security Check");

            if (pass != "321")
            {
                MessageBox.Show(
                    "Wrong password!",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            string selectedUser = lstClients.SelectedItem.ToString();

            // ✅ Actually disconnect karo
            _server.ForceDisconnect(selectedUser);

            AppendSystemMessage($"{selectedUser} disconnected by server.");
        }

        // ================= SEND FILE =================

        void BtnFile_Click(object sender, EventArgs e)
        {
            if (!_serverRunning) return;

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            byte[] fileBytes = File.ReadAllBytes(ofd.FileName);
            string fileName = Path.GetFileName(ofd.FileName);

            AppendSystemMessage($"File sent: {fileName}");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // ================= LOG — client messages =================

        void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => Log(message)));
                return;
            }

            // ✅ Client messages left side par bubble mein
            if (message.Contains(":") && !message.StartsWith("Server"))
            {
                int idx = message.IndexOf(':');
                string sender = message.Substring(0, idx).Trim();
                string text = message.Substring(idx + 1).Trim();
                AppendMessage(sender, text, isMe: false);
            }
            else
            {
                AppendSystemMessage(message);
            }

            // ✅ File mein bhi save karo
            File.AppendAllText("server_logs.txt",
                $"[{DateTime.Now}] {message}\n");
        }

        // ================= WHATSAPP STYLE BUBBLE =================

        void AppendMessage(string sender, string message, bool isMe)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendMessage(sender, message, isMe)));
                return;
            }

            string time = DateTime.Now.ToString("hh:mm tt");

            FlowLayoutPanel bubble = new FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Padding = new Padding(8, 5, 8, 5),
                Margin = new Padding(0),
                MaximumSize = new System.Drawing.Size(380, 0),
                MinimumSize = new System.Drawing.Size(100, 0),
                BackColor = isMe
                    ? (darkMode
                        ? Color.FromArgb(0, 120, 95)
                        : Color.FromArgb(37, 211, 102))
                    : (darkMode
                        ? Color.FromArgb(50, 50, 50)
                        : Color.FromArgb(235, 235, 235))
            };

            Label lblName = new Label()
            {
                Text = sender,
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = isMe
                    ? Color.FromArgb(200, 255, 200)
                    : Color.DeepSkyBlue,
                Padding = new Padding(0),
                Margin = new Padding(0, 0, 0, 1),
                BackColor = Color.Transparent
            };

            Label lblMessage = new Label()
            {
                Text = message,
                AutoSize = true,
                MaximumSize = new System.Drawing.Size(355, 0),
                Font = new Font("Segoe UI", 10),
                ForeColor = darkMode ? Color.White : Color.Black,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };

            Label lblTime = new Label()
            {
                Text = time,
                AutoSize = false,
                Width = 340,
                Font = new Font("Segoe UI", 7),
                ForeColor = darkMode
                    ? Color.LightGray
                    : Color.FromArgb(100, 100, 100),
                Padding = new Padding(0),
                Margin = new Padding(0, 1, 0, 0),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight
            };

            bubble.Controls.Add(lblName);
            bubble.Controls.Add(lblMessage);
            bubble.Controls.Add(lblTime);

            bubble.SizeChanged += (s, e) =>
            {
                if (bubble.Width > 0 && bubble.Height > 0)
                    bubble.Region = System.Drawing.Region.FromHrgn(
                        CreateRoundRectRgn(0, 0,
                            bubble.Width, bubble.Height, 12, 12));
            };

            FlowLayoutPanel wrapper = new FlowLayoutPanel()
            {
                Width = chatScroll.Width - 25,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                FlowDirection = isMe
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 2, 4, 2),
                Margin = new Padding(0)
            };

            wrapper.Controls.Add(bubble);

            int yPos = 0;
            foreach (Control c in chatPanel.Controls)
                yPos += c.Height + 4;

            wrapper.Top = yPos;
            wrapper.Left = 0;

            chatPanel.Controls.Add(wrapper);
            chatPanel.Height = yPos + wrapper.Height + 4;

            chatScroll.AutoScrollPosition = new System.Drawing.Point(
                0, chatPanel.Height);
        }

        // ================= SYSTEM MESSAGES — center =================

        void AppendSystemMessage(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AppendSystemMessage(text)));
                return;
            }

            Label lblSystem = new Label()
            {
                Text = text,
                AutoSize = false,
                Width = chatScroll.Width - 40,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };

            FlowLayoutPanel wrapper = new FlowLayoutPanel()
            {
                Width = chatScroll.Width - 25,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 1, 0, 1),
                Margin = new Padding(0)
            };

            wrapper.Controls.Add(lblSystem);

            int yPos = 0;
            foreach (Control c in chatPanel.Controls)
                yPos += c.Height + 3;

            wrapper.Top = yPos;
            chatPanel.Controls.Add(wrapper);
            chatPanel.Height = yPos + wrapper.Height + 3;

            chatScroll.AutoScrollPosition = new System.Drawing.Point(
                0, chatPanel.Height);
        }

        // ================= UI HELPERS =================

        void AddClientToList(string username)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddClientToList(username)));
                return;
            }
            lstClients.Items.Add(username);
            AppendSystemMessage($"{username} joined.");
        }

        void RemoveClientFromList(string username)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RemoveClientFromList(username)));
                return;
            }
            lstClients.Items.Remove(username);
            AppendSystemMessage($"{username} left.");
        }

        void UpdateCount(int count)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateCount(count)));
                return;
            }
            lblCount.Text = "Clients: " + count;
        }

        // ================= THEME =================

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
                chatScroll.BackColor = Color.FromArgb(20, 20, 20);
                chatPanel.BackColor = Color.FromArgb(20, 20, 20);
                lstClients.BackColor = Color.FromArgb(40, 40, 40);
                lstClients.ForeColor = Color.White;
                lblCount.ForeColor = Color.White;
                lblStatus.ForeColor = _serverRunning ? Color.Green : Color.Red;

                // ✅ Dark mein white
                btnStart.ForeColor = Color.White;
                btnStop.ForeColor = Color.White;
                btnFile.ForeColor = Color.White;
                btnTheme.ForeColor = Color.White;
                btnDisconnect.ForeColor = Color.White;
                btnPrivate.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = Color.White;
                chatScroll.BackColor = Color.WhiteSmoke;
                chatPanel.BackColor = Color.WhiteSmoke;
                lstClients.BackColor = Color.White;
                lstClients.ForeColor = Color.Black;
                lblCount.ForeColor = Color.Black;
                lblStatus.ForeColor = _serverRunning ? Color.Green : Color.Red;

                // ✅ Light mein black
                btnStart.ForeColor = Color.Black;
                btnStop.ForeColor = Color.Black;
                btnFile.ForeColor = Color.Black;
                btnTheme.ForeColor = Color.Black;
                btnDisconnect.ForeColor = Color.Black;
                btnPrivate.ForeColor = Color.Black;
            }
        }
    }
}