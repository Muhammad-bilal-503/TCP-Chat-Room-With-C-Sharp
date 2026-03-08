# 💬 Professional TCP Chat Application

![Chat Application](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows)
![Language](https://img.shields.io/badge/Language-C%23-purple?style=for-the-badge&logo=csharp)
![Framework](https://img.shields.io/badge/Framework-.NET%20Framework-blueviolet?style=for-the-badge&logo=dotnet)
![Database](https://img.shields.io/badge/Database-SQLite-green?style=for-the-badge&logo=sqlite)
![Security](https://img.shields.io/badge/Encryption-AES%20256-red?style=for-the-badge&logo=shield)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

> A **professional real-time TCP Chat Application** built with C# Windows Forms, featuring end-to-end AES encryption, SQLite database, WhatsApp-style UI, and a full client-server architecture.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technologies Used](#-technologies-used)
- [Project Structure](#-project-structure)
- [Architecture & Flow](#-architecture--flow)
- [Installation & Setup](#-installation--setup)
- [User Guide](#-user-guide)
- [Security](#-security)
- [Database Schema](#-database-schema)
- [Screenshots](#-screenshots)

---

## 🌟 Overview

This is a **full-featured TCP Chat Application** consisting of two separate projects:

| Project | Description |
|---------|-------------|
| **ServerChat** | The server-side application with a professional UI for managing clients |
| **ClientChat** | The client-side application with WhatsApp-style chat interface |

The application supports **real-time messaging**, **user authentication**, **AES encryption**, **file transfer**, **chat history**, and much more — all built from scratch using raw TCP sockets in C#.

---

## ✨ Features

### 🔐 Authentication & Security
- User **Register** and **Login** system
- Passwords stored as **SHA-256 hashes** in SQLite database
- All network packets encrypted with **AES-128 CBC** encryption
- **Duplicate login prevention** — one session per user
- Server **password-protected disconnect** feature

### 💬 Chat Features
- **Real-time messaging** over TCP
- **WhatsApp-style chat bubbles** (your messages on right, received on left)
- **Sender name** displayed above each message
- **Timestamp** shown on bottom-right of each bubble
- **Typing indicator** — `"username is typing..."` shown in real-time
- **System messages** displayed in center (connected, disconnected, etc.)

### 📁 File Transfer
- Server can **send files** to all connected clients
- Files displayed as **WhatsApp-style file bubbles** with:
  - 📎 File name
  - 📂 Open button
  - 💾 Save As button

### 🔄 Connection Management
- **Auto reconnect** — automatically tries to reconnect after disconnect
- **Manual reconnect** button
- **Safe disconnect** handling
- Connection status indicator (● Connected / ● Disconnected)

### 🗄️ Chat History
- All messages **saved to SQLite database**
- **Last 50 messages** loaded automatically on login
- Each user sees **only their own chat history**
- New users start with a **clean empty chat**

### 🎨 UI/UX
- **Dark theme** (default) and **Light theme** toggle
- WhatsApp-style **rounded bubble messages**
- Professional server dashboard with **client list**
- **Private messaging** from server to specific client
- **Broadcast messaging** to all connected clients

### 🖥️ Server Features
- Start / Stop server
- View all **connected clients** in real-time
- **Disconnect specific client** (password protected)
- Send **broadcast** messages to all clients
- Send **private** messages to specific clients
- Send **files** to all clients
- **Real-time logs** with timestamps
- Logs also saved to `server_logs.txt`

---

## 🛠️ Technologies Used

| Technology | Purpose |
|------------|---------|
| **C# .NET Framework** | Core programming language and framework |
| **Windows Forms (WinForms)** | UI for both Server and Client |
| **TCP Sockets** | Raw network communication |
| **SQLite** | Local database for users and chat history |
| **AES-128 CBC** | End-to-end encryption of all packets |
| **SHA-256** | Password hashing |
| **System.Data.SQLite** | NuGet package for SQLite integration |
| **Microsoft.VisualBasic** | InputBox dialogs |
| **GDI32.dll** | Windows API for rounded corners on bubbles |

---

## 📁 Project Structure

```
Solution/
│
├── 📦 ServerChat/
│   ├── 📂 Core/
│   │   ├── Server.cs              # TcpListener, client management, events
│   │   └── ClientHandler.cs       # Handles each client connection
│   │
│   ├── 📂 Services/
│   │   ├── AuthService.cs         # Register/Login logic
│   │   ├── DatabaseService.cs     # SQLite operations
│   │   └── EncryptionService.cs   # AES encryption + SHA-256 hashing
│   │
│   ├── Form1.cs                   # Server UI (WhatsApp-style chat + controls)
│   ├── Program.cs                 # Entry point
│   └── App.config
│
└── 📦 ClientChat/
    ├── ChatForm.cs                # Main chat UI (WhatsApp bubbles)
    ├── LoginForm.cs               # Register/Login form
    ├── EncryptionService.cs       # AES encryption (same key as server)
    ├── Form1.cs                   # Base form (unused)
    ├── Program.cs                 # Entry point → LoginForm
    └── App.config
```

---

## 🔄 Architecture & Flow

### Overall Architecture

![Server Client Architecture](Server_Client_Architecture.png)

### Authentication Flow

![Login_Sequence_Diagram](Login_Sequence_Diagram.png)

### Message Flow

![Message_Broadcast_Flow](Message_Broadcast_Flow.png)

### Typing Indicator Flow

![Typing_Indicator_Flow](Typing_Indicator_Flow.png)

---

## ⚙️ Installation & Setup

### Prerequisites

- **Visual Studio 2019/2022** (or later)
- **.NET Framework 4.7.2** or higher
- **NuGet Package**: `System.Data.SQLite`

### Step 1 — Clone the Repository

```bash
git clone https://github.com/yourusername/tcp-chat-app.git
cd tcp-chat-app
```

### Step 2 — Install NuGet Package

Open **Package Manager Console** in Visual Studio:

```powershell
# For ServerChat project
Install-Package System.Data.SQLite
```

### Step 3 — Build the Solution

```
Build → Rebuild Solution (Ctrl + Shift + B)
```

### Step 4 — Run Server First

```
Right-click ServerChat → Set as Startup Project → Run (F5)
```

Click **Start** button → Server starts on port `55555`

### Step 5 — Run Client

```
Right-click ClientChat → Set as Startup Project → Run (F5)
```

> ⚠️ **Note**: Run Server **before** running the Client.

---

## 📖 User Guide

### 👤 Registering an Account

1. Open **ClientChat**
2. Enter your desired **Username** and **Password**
3. Click **Register**
4. Success message will appear → Now click **Login**

### 💬 Sending Messages

1. Login with your credentials
2. Type your message in the **text box** at the bottom
3. Press **Enter** or click **Send**
4. Your message appears on the **right side** (green bubble)
5. Received messages appear on the **left side** (gray bubble)

### 📁 Receiving Files

When server sends a file:
1. A **file bubble** appears in chat
2. Click **📂 Open** to open the file directly
3. Click **💾 Save As** to save the file to your computer

### 🔄 Reconnecting

- Click **Reconnect** button if connection is lost
- Or wait — the app will **auto-reconnect** after disconnect

### 🎨 Switching Theme

- Click **Toggle Theme** to switch between **Dark** and **Light** mode

---

### 🖥️ Server Guide

| Action | How To |
|--------|--------|
| Start Server | Click **Start** button |
| Stop Server | Click **Stop** button |
| Broadcast Message | Type in bottom box → Click **Broadcast** |
| Send Private Message | Select client from list → Type message → Click **Send Private** |
| Send File | Click **Send File** → Select file |
| Disconnect Client | Select client → Click **Disconnect** → Enter password `321` |
| Toggle Theme | Click **Toggle Theme** |

---

## 🔒 Security

### AES-128 CBC Encryption

All data transmitted between client and server is encrypted:

```
Plain Text  ──▶  AES Encrypt  ──▶  Base64 String  ──▶  Network
Network     ──▶  Base64 Decode ──▶  AES Decrypt   ──▶  Plain Text
```

- **Key**: 16-character shared secret key
- **IV**: 16-character initialization vector
- **Mode**: CBC (Cipher Block Chaining)
- **Padding**: PKCS7

### Password Hashing

Passwords are **never stored in plain text**:

```
Password ──▶ SHA-256 Hash ──▶ Stored in SQLite
```

### What is Encrypted?

| Data | Encrypted? |
|------|-----------|
| Login credentials | ✅ Yes |
| Register credentials | ✅ Yes |
| Chat messages | ✅ Yes |
| Typing indicators | ✅ Yes |
| Server responses | ❌ No (AUTH_REQUIRED, AUTH_SUCCESS etc.) |

---

## 🗄️ Database Schema

Database file: `chat.db` (auto-created in Debug folder)

### Users Table

```sql
CREATE TABLE Users (
    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
    Username     TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL
);
```

### Messages Table

```sql
CREATE TABLE Messages (
    Id       INTEGER PRIMARY KEY AUTOINCREMENT,
    Sender   TEXT NOT NULL,
    Receiver TEXT NOT NULL DEFAULT 'all',
    Message  TEXT NOT NULL,
    SentAt   TEXT NOT NULL
);
```

---

## 📦 Packet Protocol

| Packet | Direction | Description |
|--------|-----------|-------------|
| `AUTH_REQUIRED` | Server → Client | Request authentication |
| `LOGIN\|user\|pass` | Client → Server | Login for chat session |
| `LOGIN_CHECK\|user\|pass` | Client → Server | Validate credentials only |
| `REGISTER\|user\|pass` | Client → Server | Register new account |
| `AUTH_SUCCESS` | Server → Client | Login successful |
| `AUTH_FAILED` | Server → Client | Login failed |
| `LOGIN_SUCCESS` | Server → Client | Credentials valid |
| `LOGIN_FAILED` | Server → Client | Credentials invalid |
| `DUPLICATE_USER` | Server → Client | User already logged in |
| `MSG:text` | Both | Chat message |
| `FILE:name:size` | Server → Client | File transfer header |
| `TYPING:user` | Client → Server | Typing indicator |
| `HISTORY:sender:msg:time` | Server → Client | Chat history item |

---

## 🚀 Future Improvements

- [ ] Group Chat Rooms
- [ ] Message Read Receipts (double tick)
- [ ] Online/Offline Status indicators
- [ ] Sound Notifications
- [ ] Message Search in history
- [ ] User Profile & Avatar
- [ ] File Transfer Progress Bar
- [ ] Server Dashboard with statistics

---

## 👨‍💻 Author

**Muhammad Bilal**

> Built with ❤️ using C# Windows Forms and raw TCP Sockets

---

## 📄 License

This project is licensed under the **MIT License**.

```
MIT License — Free to use, modify, and distribute
```

---

## 🙏 Acknowledgements

- **System.Data.SQLite** — SQLite for .NET
- **Windows GDI32 API** — Rounded corners for chat bubbles
- **AES Cryptography** — .NET System.Security.Cryptography
