using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ServerChat.Services
{
    public class EncryptionService
    {
        // ✅ Dono sides par same key honi chahiye
        private static readonly string AesKey = "MySecretKey12345";  // 16 characters = 128 bit
        private static readonly string AesIV = "MySecretIV123456";  // 16 characters

        // ✅ Password hash — pehle se same hai
        public static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        // ✅ Encrypt — plain text → base64 string
        public static string Encrypt(string plainText)
        {
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(AesKey);
                byte[] iv = Encoding.UTF8.GetBytes(AesIV);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch
            {
                return plainText;
            }
        }

        // ✅ Decrypt — base64 string → plain text
        public static string Decrypt(string cipherText)
        {
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(AesKey);
                byte[] iv = Encoding.UTF8.GetBytes(AesIV);
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(cipherBytes))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return cipherText;
            }
        }
    }
}