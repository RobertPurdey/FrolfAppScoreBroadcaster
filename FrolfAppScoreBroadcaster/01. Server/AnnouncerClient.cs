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
        private Thread handlerThread;

        public AnnouncerClient(TcpClient newClient, AppUserModel newAnnouncer)
        {
            client    = newClient;
            announcer = newAnnouncer;

            handlerThread = new Thread( () => AnnounceGameUpdate() );
            handlerThread.Start();
        }

        public void Reconnect(TcpClient newClient)
        {
            client.GetStream().Close();
            client.Close();

            client = newClient;

            RestartThread();
        }

        public void Disconnect()
        {
            client.GetStream().Close();
            client.Close();

            //DisconnectSpecs();
        }

        private void DisconnectSpecs()
        {
            foreach ( var spec in spectators )
            {
                spec.Disconnect();
            }
        }

        private void RestartThread()
        {
            handlerThread = new Thread(() => AnnounceGameUpdate());
            handlerThread.Start();
        }

        public Task AnnounceGameUpdate()
        {
            var lastMsg = string.Empty;

            while (lastMsg != "-------END")
            {
                try
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
                        spectator.TriggerGameUpdate(gameResultUpdate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Announcer disconnected...");
                    break;
                }
            }

            Console.WriteLine("Task finished...");
            return Task.FromResult(1);
        }
    }
}
