using FrolfAppScoreBroadcaster._02._Api.Models;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FrolfAppScoreBroadcaster._01._Server
{
    public class SpectatorClient
    {
        public AppUserModel spectator;
        TcpClient client;

        public SpectatorClient() { }

        public SpectatorClient(TcpClient client, AppUserModel newSpectator)
        {
            this.client     = client;
            this.spectator  = newSpectator;
        }

        public void TriggerGameUpdate(GameResultModel gameUpdate)
        {
            Thread sendUpdate = new Thread(() => SendGameUpdate(gameUpdate));
            sendUpdate.Start();
        }


        public Task SendGameUpdate(GameResultModel gameUpdate)
        {
            var stream = client.GetStream();

            // todo: encrypt message
            //spectator.RsaPubXml
            var encryptGameUpdate  = ModelEncryptor.Encrypt(spectator.RsaPubXml, gameUpdate);
            var gameUpdateJson     = JsonConvert.SerializeObject(encryptGameUpdate);

            var gameUpdateSize      = gameUpdateJson.Length.ToString().PadLeft(8, '0');
            var gameUpdateSizeBytes = Encoding.ASCII.GetBytes(gameUpdateSize);

            var updateBytes = Encoding.ASCII.GetBytes(gameUpdateJson);

            stream.Write(gameUpdateSizeBytes);
            stream.Write(updateBytes);

            Console.WriteLine("Send game update finished...");
            return Task.FromResult(1);
        }

    }
}
