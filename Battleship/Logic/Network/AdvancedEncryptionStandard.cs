using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Battleship.Logic.Network
{
    public class AdvancedEncryptionStandard
    {
        public static readonly string pattern = @"[a-zA-Z0-9]";

        public static string CreateKey()
        {
            return RandomString(16);
        }

        private static string RandomString(int length)
        {
            // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
            var random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string? Encrypt(string plaintext, string key)
        {
            // https://ironpdf.com/de/blog/net-help/csharp-aes-encryption/
            if (!Regex.IsMatch(key, pattern))
                return null;

            // Create a new instance of the AES encryption algorithm
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16]; // Initialization vector (IV)
                                        // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                // Create the streams used for encryption
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create a CryptoStream using the encryptor
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plaintext);
                        }
                    }
                    // Store the encrypted data in the public static byte array
                    var encryptedData = ms.ToArray();

                    return Convert.ToBase64String(encryptedData);
                }
            }
        }

        // Method to decrypt data
        public static string? Decrypt(string ciphertext, string key)
        {
            // https://ironpdf.com/de/blog/net-help/csharp-aes-encryption/
            if (!Regex.IsMatch(key, pattern))
                return null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = new byte[16]; // Initialization vector (IV)
                                        // Create a decryptor to perform the stream transform
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                // Create the streams used for decryption
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(ciphertext)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static void test()
        {
            // Just an example to show how to use it proper.
            string plaintext = "This is some sensitive data!";
            string key = "abcdefghijklmnop";

            Debug.WriteLine(plaintext);
            Debug.WriteLine($"key: {key} ({Regex.IsMatch(key, pattern)})");

            var ciphertext = Encrypt(plaintext, key);

            Debug.WriteLine(ciphertext);

            var decrypted = Decrypt(ciphertext!, key);

            Debug.WriteLine(decrypted);
        }
    }
}
