using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ServerChat.Services
{
    public class DatabaseService
    {
        private string connectionString =
            $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chat.db")};Version=3;Pooling=True;";

        public DatabaseService()
        {
            CreateUsersTable();
            CreateMessagesTable();
        }

        // ================= USERS TABLE =================

        private void CreateUsersTable()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"CREATE TABLE IF NOT EXISTS Users (
                    Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username     TEXT UNIQUE,
                    PasswordHash TEXT
                );";
                using (var command = new SQLiteCommand(query, connection))
                    command.ExecuteNonQuery();
            }
        }

        // ================= MESSAGES TABLE =================

        private void CreateMessagesTable()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"CREATE TABLE IF NOT EXISTS Messages (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    Sender    TEXT    NOT NULL,
                    Message   TEXT    NOT NULL,
                    SentAt    TEXT    NOT NULL
                );";
                using (var command = new SQLiteCommand(query, connection))
                    command.ExecuteNonQuery();
            }
        }

        // ================= SAVE MESSAGE =================

        public void SaveMessage(string sender, string message)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Messages (Sender, Message, SentAt)
                                 VALUES (@sender, @message, @sentAt)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@sender",  sender);
                    command.Parameters.AddWithValue("@message", message);
                    command.Parameters.AddWithValue("@sentAt",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.ExecuteNonQuery();
                }
            }
        }

        // ================= LOAD MESSAGES =================

        public List<(string Sender, string Message, string Time)> GetRecentMessages(int count = 50)
        {
            var messages = new List<(string, string, string)>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = $@"SELECT Sender, Message, SentAt
                                  FROM Messages
                                  ORDER BY Id DESC
                                  LIMIT {count}";
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        messages.Add((
                            reader["Sender"].ToString(),
                            reader["Message"].ToString(),
                            reader["SentAt"].ToString()
                        ));
                    }
                }
            }

            // ✅ Reverse karo — purane pehle, naye baad mein
            messages.Reverse();
            return messages;
        }

        // ================= USERS =================

        public bool UserExists(string username)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username=@username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public void RegisterUser(string username, string passwordHash)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Users (Username, PasswordHash)
                                 VALUES (@username, @passwordHash)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username",     username);
                    command.Parameters.AddWithValue("@passwordHash", passwordHash);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool ValidateUser(string username, string passwordHash)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT COUNT(*) FROM Users
                                 WHERE Username=@username
                                 AND PasswordHash=@passwordHash";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username",     username);
                    command.Parameters.AddWithValue("@passwordHash", passwordHash);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
    }
}