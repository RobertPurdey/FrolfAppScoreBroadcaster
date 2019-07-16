using FrolfAppScoreBroadcaster._01._Server;
using FrolfAppScoreBroadcaster._02._Api.Models;
using FrolfAppScoreBroadcaster._04._Encryption;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FrolfAppScoreBroadcaster._02._Api
{
    public class ApiContactor
    {
        public const string URL = "http://54.241.250.34/api/";
        //public const string URL = "http://192.168.1.88:53740/api/";

        private static string BuildApiRoute(string path)
        {
            if ( path == null ) throw new ArgumentNullException("path");

            return URL + path;
        }

        public static async Task<EncryptModel> GetUserInfo(string token)
        {
            var apiPath           = BuildApiRoute("appusers/info/internal");
            EncryptModel userInfo = null;

            using ( var request = new HttpRequestMessage(HttpMethod.Get, apiPath) )
            using ( var client  = new HttpClient() )
            {
                client.DefaultRequestHeaders.Authorization  = new AuthenticationHeaderValue("Bearer", token);
                HttpResponseMessage response                = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    userInfo = JsonConvert.DeserializeObject<EncryptModel>(await response.Content.ReadAsStringAsync());
                }
            }

            return userInfo;
        }

        public static async Task<bool> CanCommandBeHandled(ClientRequestModel clientRequest)
        {
            var apiPath      = BuildApiRoute("games/" + clientRequest.Command);
            var encryptModel = new EncryptModel();
            var canHandle    = false;

            var gameIdModel = new IdModel { IdKey = clientRequest.GameId };

            var encryptGameId         = ModelEncryptor.Encrypt(RsaKeyInfo.GetRsaParameters(), gameIdModel);
            var encryptGameJsonString = JsonConvert.SerializeObject(encryptGameId);

            using ( var request = new HttpRequestMessage(HttpMethod.Post, apiPath) )
            using ( var client  = new HttpClient() )
            {
                client.DefaultRequestHeaders.Authorization  = new AuthenticationHeaderValue("Bearer", clientRequest.Token);
                HttpContent content                         = new StringContent(encryptGameJsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response                = await client.PostAsync(apiPath, content);

                if (response.IsSuccessStatusCode)
                {
                    encryptModel = JsonConvert.DeserializeObject<EncryptModel>(await response.Content.ReadAsStringAsync());
                    canHandle    = ModelEncryptor.DecryptFromServer<BoolModel>(encryptModel).Value;
                }
            }

            return canHandle;
        }
    }
}
