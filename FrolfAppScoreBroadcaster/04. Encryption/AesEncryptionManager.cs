using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FrolfAppScoreBroadcaster._04._Encryption
{
    public class AesEncryptionManager
    {
        public static string GenerateKey()
        {
            var provider  = new RNGCryptoServiceProvider();
            var newKey    = new byte[16];

            provider.GetBytes(newKey);

            return Convert.ToBase64String(newKey);
        }

        public static string Encrypt(string key, string msg)
        {
            using (var aes = GetAesProvider(key))
            {
                aes.GenerateIV();

                var iv          = aes.IV;
                var inputBytes  = Encoding.UTF8.GetBytes(msg);

                using (var encryptor    = aes.CreateEncryptor(aes.Key, iv))
                using (var cipherStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(cipherStream, encryptor, CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        // Prepend IV to data
                        cipherStream.Write(iv, 0, 16);
                        binaryWriter.Write(inputBytes);
                        cryptoStream.FlushFinalBlock();
                    }

                    return Convert.ToBase64String(cipherStream.ToArray());
                }
            }
        }

        public static string Decrypt(string key, string encryptedMsg)
        {
            using (var aes = GetAesProvider(key))
            {
                var encryptedMsgBytes = Convert.FromBase64String(encryptedMsg);

                //get first 16 bytes (which is the init vector) of the message and use it to decrypt
                var iv = new byte[16];
                Array.Copy(encryptedMsgBytes, 0, iv, 0, iv.Length);

                using (var decryptor    = aes.CreateDecryptor(aes.Key, iv))
                using (var cipherStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Write))
                    using (var binaryWriter = new BinaryWriter(cryptoStream))
                    {
                        // Decrypt Cipher Text from Message
                        binaryWriter.Write(encryptedMsgBytes, iv.Length, encryptedMsgBytes.Length - iv.Length);
                    }

                    return Encoding.UTF8.GetString(cipherStream.ToArray());
                }
            }
        }

        private static AesCryptoServiceProvider GetAesProvider(string key)
        {
            return new AesCryptoServiceProvider
            {
                Key      = Convert.FromBase64String(key),
                Mode     = CipherMode.CBC,
                Padding  = PaddingMode.PKCS7
            };
        }
    }
}
