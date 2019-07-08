using FrolfAppScoreBroadcaster._02._Api.Models;
using FrolfAppScoreBroadcaster._03._Communication;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FrolfAppScoreBroadcaster._01._Server
{
    public class AnnouncerClient
    {
        public AppUserModel announcer;
        TcpClient client;

        public HashSet<SpectatorClient> spectators = new HashSet<SpectatorClient>();

        public AnnouncerClient(TcpClient newClient, AppUserModel newAnnouncer)
        {
            client    = newClient;
            announcer = newAnnouncer;

            Thread handleAnnounce = new Thread(() => AnnounceGameUpdate());
            handleAnnounce.Start();
        }

        public Task AnnounceGameUpdate()
        {
            var lastMsg = string.Empty;

            while (lastMsg != "-------END")
            {
                var stream = client.GetStream();
                lastMsg    = Messages.GetMessage(stream, 8);

                // read game update size
                var gameSizeMsg = lastMsg;
                var gameSize    = Convert.ToInt32(gameSizeMsg);

                Console.WriteLine("Game Update Size: " + gameSize + "B");
                Console.WriteLine();

                // read game update
                var gameUpdate       = Messages.GetMessage(stream, gameSize);
                var gameResultUpdate = ModelEncryptor.Decrypt<GameResultModel>(gameUpdate);

                foreach (var spectator in spectators)
                {
                    spectator.SendGameUpdate(gameResultUpdate);
                }
            }

            Console.WriteLine("Task finished...");
            return Task.FromResult(1);
        }
    }
}
