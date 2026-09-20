using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Nanodogs.API.Nanosaves
{
    public static class NanoSaves
    {
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("FixedSaltValue123"); // constant salt
        private static readonly byte[] Key;

        static NanoSaves()
        {
            // Create a stable key from an obfuscated base string + per-install ID
            string password = GetObfuscatedPassword_PerInstall();
            Key = GenerateKeyFromPassword(password, Salt);
        }

        /// <summary>
        /// Saves encrypted data to a binary file with a unique IV prepended.
        /// </summary>
        public static void SaveData(string key, string data)
        {
            string path = Path.Combine(Application.persistentDataPath, key + ".NanoSave");
            string jsonData = JsonUtility.ToJson(new NanoSaveData { data = data });

            byte[] iv = GenerateRandomIV();
            byte[] encrypted = EncryptStringToBytes_Aes(jsonData, Key, iv);

            byte[] combined = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, combined, iv.Length, encrypted.Length);

            File.WriteAllBytes(path, combined);
            Debug.Log($"Saved encrypted data ({combined.Length} bytes) to {path}");
        }

        /// <summary>
        /// Loads and decrypts data from a binary file using the stored IV.
        /// </summary>
        public static string LoadData(string key)
        {
            string path = Path.Combine(Application.persistentDataPath, key + ".NanoSave");
            if (!File.Exists(path))
            {
                Debug.LogWarning("No save file found: " + key);
                return null;
            }

            byte[] combined = File.ReadAllBytes(path);
            Debug.Log($"Read encrypted file length: {combined.Length} bytes");

            if (combined.Length < 17)
            {
                Debug.LogError("Save file corrupted or empty.");
                return null;
            }

            byte[] iv = new byte[16];
            Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);

            byte[] encrypted = new byte[combined.Length - iv.Length];
            Buffer.BlockCopy(combined, iv.Length, encrypted, 0, encrypted.Length);

            try
            {
                string decrypted = DecryptStringFromBytes_Aes(encrypted, Key, iv);
                NanoSaveData saveData = JsonUtility.FromJson<NanoSaveData>(decrypted);
                return saveData.data;
            }
            catch (CryptographicException e)
            {
                Debug.LogError($"Failed to decrypt save file: {e.Message}");
                return null;
            }
        }

        #region Encryption Helpers

        private static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                    swEncrypt.Flush();
                    csEncrypt.FlushFinalBlock();
                    return msEncrypt.ToArray();
                }
            }
        }

        private static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                using (var msDecrypt = new MemoryStream(cipherText))
                using (var csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                using (var srDecrypt = new StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }

        private static byte[] GenerateKeyFromPassword(string password, byte[] salt)
        {
            using (var keyGen = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256))
            {
                return keyGen.GetBytes(32); // AES-256
            }
        }

        private static byte[] GenerateRandomIV()
        {
            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }
            return iv;
        }

        #endregion

        #region Obfuscation + Persistent Key

        /// <summary>
        /// Returns a per-install obfuscated password to generate the AES key.
        /// This ensures encryption key is consistent between sessions but not stored in plaintext.
        /// </summary>
        private static string GetObfuscatedPassword_PerInstall()
        {
            // store install ID in a file, not PlayerPrefs (safer)
            string installId = GetOrCreateInstallID();

            // Base obfuscated literal
            char[] baseChars = { 'd', 'o', 'n', 'o', 't', 't', 'r', 'y', 'd', 'e', 'o', 'b', 'f', 'u', 's', 'c', 'a', 't', 'e', 't', 'h', 'i', 's', '.' };
            Array.Reverse(baseChars);

            const byte xorKey = 0x2A;
            for (int i = 0; i < baseChars.Length; i++)
                baseChars[i] = (char)(baseChars[i] ^ xorKey);

            string basePart = new string(baseChars);
            return basePart + "_" + installId;
        }

        private static string GetOrCreateInstallID()
        {
            string keyPath = Path.Combine(Application.persistentDataPath, "NanoKey.id");

            if (File.Exists(keyPath))
            {
                return File.ReadAllText(keyPath);
            }

            string newId = Guid.NewGuid().ToString("N");
            File.WriteAllText(keyPath, newId);
            return newId;
        }

        #endregion
    }

    [Serializable]
    public class NanoSaveData
    {
        public string data;
    }
}
