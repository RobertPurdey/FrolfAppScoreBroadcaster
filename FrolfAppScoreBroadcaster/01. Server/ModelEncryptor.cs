using FrolfAppScoreBroadcaster._02._Api.Models;
using FrolfAppScoreBroadcaster._04._Encryption;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace FrolfAppScoreBroadcaster._01._Server
{
    public class ModelEncryptor
    {
        public static EncryptModel Encrypt<TModel>(RSAParameters rsaParams, TModel model) where TModel : class
        {
            string aesKey = AesEncryptionManager.GenerateKey();

            byte[] encryptedAesKey       = RsaEncryptionManager.Encrypt(rsaParams, aesKey);
            string encryptedJsonBase64   = AesEncryptionManager.Encrypt(aesKey, JsonConvert.SerializeObject(model));
            string encryptedAesKeyBase64 = Convert.ToBase64String(encryptedAesKey);

            return new EncryptModel
            {
                EncryptedAesKey = encryptedAesKeyBase64,
                EncryptedJson   = encryptedJsonBase64
            };
        }

        public static EncryptModel Encrypt<TModel>(string rsaPubXml, TModel model) where TModel : class
        {

            var xDoc      = XDocument.Parse(rsaPubXml);
            var root      = xDoc.Root;

            var keyParams = root.Elements().ToList();
            var modulus   = keyParams.ElementAt(0).Value;
            var exponent  = keyParams.ElementAt(1).Value;

            var rsaParams = new RSAParameters
            {
                Modulus  = Convert.FromBase64String(modulus),
                Exponent = Convert.FromBase64String(exponent)
            };

            return Encrypt(rsaParams, model);
        }

        public static TModel Decrypt<TModel>(EncryptModel model) where TModel : class
        {
            var encryptedAesKeyBytes = Convert.FromBase64String(model.EncryptedAesKey);
            var serverRsaPrivKey     = RsaKeyInfo.GetRsaParameters();

            var decrytedAesKeyBytes  = RsaEncryptionManager.Decrypt(serverRsaPrivKey, encryptedAesKeyBytes);
            var decrytedAesKeyBase64 = Convert.ToBase64String(decrytedAesKeyBytes);
            var decryptedJsonBytes   = AesEncryptionManager.Decrypt(decrytedAesKeyBase64, model.EncryptedJson);

            return JsonConvert.DeserializeObject<TModel>(decryptedJsonBytes);
        }

        public static TModel DecryptFromServer<TModel>(EncryptModel model) where TModel : class
        {
            var encryptedAesKeyBytes = Convert.FromBase64String(model.EncryptedAesKey);
            var serverRsaPrivKey     = RsaKeyInfo.GetRsaParameters();

            var decrytedAesKeyBytes  = RsaEncryptionManager.Decrypt(serverRsaPrivKey, encryptedAesKeyBytes);
            var decrytedAesKeyBase64 = Encoding.UTF8.GetString(decrytedAesKeyBytes);
            var decryptedJsonBytes   = AesEncryptionManager.Decrypt(decrytedAesKeyBase64, model.EncryptedJson);

            return JsonConvert.DeserializeObject<TModel>(decryptedJsonBytes);
        }

        public static TModel Decrypt<TModel>(string model) where TModel : class
        {
            var encryptModel = JsonConvert.DeserializeObject<EncryptModel>(model);

            return Decrypt<TModel>(encryptModel);
        }
    }
}
