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
        private TcpClient client;

        public SpectatorClient() { }

        public SpectatorClient(TcpClient client, AppUserModel newSpectator)
        {
            this.client     = client;
            this.spectator  = newSpectator;
        }

        public void TriggerGameUpdate(GameResultModel gameUpdate)
        {
            Thread handlerThread = new Thread( () => SendGameUpdate(gameUpdate) );
            handlerThread.Start();
        }

        public void Reconnect(TcpClient newClient)
        {
            Disconnect();
            client = newClient;
        }

        public void Disconnect()
        {
            client.GetStream().Close();
            client.Close();
        }

        public Task SendGameUpdate(GameResultModel gameUpdate)
        {
            var stream = client.GetStream();

            try
            { 
                // todo: encrypt message
                //spectator.RsaPubXml
                var encryptGameUpdate  = ModelEncryptor.Encrypt(spectator.RsaPubXml, gameUpdate);
                var gameUpdateJson     = JsonConvert.SerializeObject(encryptGameUpdate);

                var gameUpdateSize      = gameUpdateJson.Length.ToString().PadLeft(8, '0');
                var gameUpdateSizeBytes = Encoding.ASCII.GetBytes(gameUpdateSize);

                var updateBytes = Encoding.ASCII.GetBytes(gameUpdateJson);

                stream.Write(gameUpdateSizeBytes);
                stream.Write(updateBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Spectator disconnected...");
            }

            Console.WriteLine("Game update sent...");
            return Task.FromResult(1);
        }

    }
}
