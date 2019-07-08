using System.Security.Cryptography;
using System.Text;

namespace FrolfAppScoreBroadcaster._04._Encryption
{
    public class RsaEncryptionManager
    {
        public static byte[] Encrypt(RSAParameters rsaParams, string msg)
        {
            var encryptedMsg = new byte[] { };

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.ImportParameters(rsaParams);
                encryptedMsg = rsa.Encrypt(Encoding.UTF8.GetBytes(msg), false);
            }

            return encryptedMsg;
        }

        public static byte[] Decrypt(RSAParameters rsaParams, byte[] encryptedMsg)
        {
            var decryptedBytes = new byte[] { };

            using (var rsa = new RSACryptoServiceProvider(2048))
            {

                rsa.ImportParameters(rsaParams);
                decryptedBytes = rsa.Decrypt(encryptedMsg, false);
            }

            return decryptedBytes;
        }
    }
}
