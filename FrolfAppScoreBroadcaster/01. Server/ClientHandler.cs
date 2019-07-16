using FrolfAppScoreBroadcaster._01._Server;
using FrolfAppScoreBroadcaster._02._Api;
using FrolfAppScoreBroadcaster._02._Api.Models;
using FrolfAppScoreBroadcaster._03._Communication;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FrolfAppScoreBroadcaster.Server
{
    public class ClientHandler
    {
        private static Dictionary<Guid, AnnouncerClient> gameAnnouncers = new Dictionary<Guid, AnnouncerClient>();
        private static HashSet<string> commands = new HashSet<string>() { "SPECTATE", "ANNOUNCE" };

        public async static Task<Task> Handle(TcpClient client)
        {
            var stream = client.GetStream();
    
            var clientRequestMsg        = ReadClientRequestMsg(stream);
            var clientRequest           = DecryptClientRequest(clientRequestMsg);
            var encryptRequesterInfo    = await GetRequesterUserInfo(clientRequest.Token);

            if (encryptRequesterInfo == null )
            {
                TrashClient(client);
            }

            var requesterInfo = DecryptRequesterUserInfo(encryptRequesterInfo);

            var isValid = IsCommandValid(clientRequest.Command);
            if ( !isValid )
            {
                TrashClient(client);
            }

            var canHandle = await HandleCommand(clientRequest);
            if ( !canHandle )
            {
                TrashClient(client);
            }

            AddClient(clientRequest, requesterInfo, client);

            return Task.FromResult(1);
        }
 
        private static string ReadClientRequestMsg(NetworkStream netStream)
        {
            var clientRequestSizeMsg = Messages.GetMessage(netStream, 4);
            var clientRequestSize    = Convert.ToInt32(clientRequestSizeMsg);

            Console.WriteLine("Token Size: " + clientRequestSize + "B");
            Console.WriteLine();

            // read encrypted request
            var clientRequest = Messages.GetMessage(netStream, clientRequestSize);

            return clientRequest;
        }

        private static ClientRequestModel DecryptClientRequest(string clientRequest)
        {
            var clientRequestDecyrpted = ModelEncryptor.Decrypt<ClientRequestModel>(clientRequest);

            Console.WriteLine("Client request received");
            Console.WriteLine();

            return clientRequestDecyrpted;
        }

        private static async Task<EncryptModel> GetRequesterUserInfo(string token)
        {
            var userInfoEncrypted = await ApiContactor.GetUserInfo(token);

            return userInfoEncrypted;
        }

        private static AppUserModel DecryptRequesterUserInfo(EncryptModel model)
        {
            return ModelEncryptor.DecryptFromServer<AppUserModel>(model);
        }

        private static bool IsCommandValid(string command)
        {
            return commands.Contains(command);
        }

        private static async Task<bool> HandleCommand(ClientRequestModel clientRequest)
        {
            var canHandle = await ApiContactor.CanCommandBeHandled(clientRequest);

            return canHandle;
        }

        private static void AddClient(ClientRequestModel request, AppUserModel requesterUserInfo, TcpClient client)
        {
            var gameId = request.GameId;

            if (request.Command == "SPECTATE")
            {
                lock (gameAnnouncers)
                {
                    var newSpec = new SpectatorClient(client, requesterUserInfo);

                    // only add if game being announced
                    if ( gameAnnouncers.ContainsKey(gameId) )
                    {
                        var announcer = gameAnnouncers[gameId];

                        // remove lingering spectator
                        announcer.spectators.RemoveWhere(spec => spec.spectator.IdKey == requesterUserInfo.IdKey);
                        announcer.spectators.Add(newSpec);

                        Console.WriteLine("Added Spec!");
                    }
                }
            }
            else if (request.Command == "ANNOUNCE")
            {
                lock (gameAnnouncers)
                {
                    var newAnnouncer = new AnnouncerClient(client, requesterUserInfo);

                    if ( gameAnnouncers.ContainsKey(gameId) )
                    {
                        gameAnnouncers.Remove(gameId);
                    }

                    gameAnnouncers.Add(gameId, newAnnouncer);
                    Console.WriteLine("Added Announcer!");
                }
            }
        }

        private static void TrashClient(TcpClient client)
        {
            client.GetStream().Close();
            client.Close();
        }
    }
}
