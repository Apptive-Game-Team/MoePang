using System;
using System.Security.Cryptography;
using System.Text;

namespace _01.Scripts._10.System
{
    public static class SaveDataEncryption
    {
        private static readonly byte[] EncryptionKey =
            Convert.FromBase64String("mX7q2V9nL4pR8sT1wY6aC0dF3gH5jK9u");

        private static readonly byte[] AuthenticationKey =
            Convert.FromBase64String("Q8vN2zL5cR1xM7kT4pW9aB6dF0hJ3sY2");

        private const string VersionPrefix = "MP01:";

        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = EncryptionKey;
            aes.GenerateIV();

            using ICryptoTransform encryptor =
                aes.CreateEncryptor();
            byte[] cipherBytes =
                encryptor.TransformFinalBlock(
                    plainBytes, 0, plainBytes.Length);
            
            byte[] payload = new byte[
                aes.IV.Length + cipherBytes.Length];

            Buffer.BlockCopy(
                aes.IV, 0, payload, 0, aes.IV.Length);

            Buffer.BlockCopy(
                cipherBytes, 0, payload,
                aes.IV.Length, cipherBytes.Length);
            
            byte[] authenticatedBytes =
                Encoding.UTF8.GetBytes(
                    VersionPrefix + Convert.ToBase64String(payload));

            using HMACSHA256 hmac =
                new HMACSHA256(AuthenticationKey);
            byte[] tag = hmac.ComputeHash(authenticatedBytes);

            return VersionPrefix
                   + Convert.ToBase64String(payload)
                   + ":"
                   + Convert.ToBase64String(tag);
        }

        public static string Decrypt(string encryptedText)
        {
            string[] parts = encryptedText.Split(':');

            if (parts.Length != 3 ||
                parts[0] != "MP01")
            {
                throw new CryptographicException(
                    "Invalid save data format.");
            }

            byte[] payload = Convert.FromBase64String(parts[1]);
            byte[] receivedTag = Convert.FromBase64String(parts[2]);

            byte[] authenticatedBytes =
                Encoding.UTF8.GetBytes(parts[0] + ":" + parts[1]);

            byte[] expectedTag;

            using (HMACSHA256 hmac =
                   new HMACSHA256(AuthenticationKey))
            {
                expectedTag = hmac.ComputeHash(authenticatedBytes);
            }
            
            if (!FixedTimeEquals(expectedTag, receivedTag))
            {
                throw new CryptographicException(
                    "Save data authentication failed.");
            }

            using Aes aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = EncryptionKey;

            int ivLength = aes.BlockSize / 8;

            if (payload.Length <= ivLength ||
                (payload.Length - ivLength) % ivLength != 0)
            {
                throw new CryptographicException(
                    "Invalid encrypted payload.");
            }

            byte[] iv = new byte[ivLength];
            byte[] cipherBytes =
                new byte[payload.Length - ivLength];

            Buffer.BlockCopy(
                payload, 0, iv, 0, ivLength);

            Buffer.BlockCopy(
                payload, ivLength, cipherBytes,
                0, cipherBytes.Length);

            aes.IV = iv;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            
            byte[] plainBytes = decryptor.TransformFinalBlock(
                    cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            int diff = 0;

            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}