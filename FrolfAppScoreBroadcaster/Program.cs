using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpListenerTest
{
    class Program
    {
        static Dictionary<Guid, Announcer> gameAnnouncers = new Dictionary<Guid, Announcer>();

        static HashSet<string> commands = new HashSet<string>() { "SPECTATE", "ANNOUNCE" };


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Start the server  
            Thread listeningThread = new Thread(TcpHelper.StartServer);
            listeningThread.Start();
        }

        public static class MessageReceive
        {
            public static string GetMessage(NetworkStream stream, int size)
            {
                if (stream == null) throw new ArgumentNullException("stream");

                var bytesRead   = 0;
                var message     = new byte[size];

                while (bytesRead < size)
                {
                    bytesRead += stream.Read(message, bytesRead, size - bytesRead);
                }

                return Encoding.ASCII.GetString(message);
            }
        }

        class ClientValidator
        {
            private static string GetMessage(NetworkStream stream, int size)
            {
                if (stream == null) throw new ArgumentNullException("stream");

                var bytesRead = 0;
                var message = new byte[size];

                while (bytesRead < size)
                {
                    bytesRead += stream.Read(message, bytesRead, size - bytesRead);
                }

                return Encoding.ASCII.GetString(message);
            }

            public static async void ValidateClient(TcpClient newClient)
            {
                var buffer = new byte[1024];
                var stream = newClient.GetStream();

                // read the size of the api key being sent (the size will always be contained to 4 bytes) size is written in bytes)
                var tokenSizeMsg = GetMessage(stream, 4);
                var tokenSize = Convert.ToInt32(tokenSizeMsg);
                Console.WriteLine("Token Size: " + tokenSize + "B");
                Console.WriteLine();

                // read API key
                var apiKeyMsg = GetMessage(stream, tokenSize);
                Console.WriteLine("Token: " + apiKeyMsg);
                Console.WriteLine();

                // validate API key
                var isKeyValid  = await CheckValidApiKey("allowedBroadcastAccess", apiKeyMsg);
                var validMsg    = isKeyValid ? "YES" : "NO";

                Console.WriteLine("Token valid? " + validMsg);
                Console.WriteLine();

                // Deny client for invalid key
                if ( !isKeyValid)
                {
                    TrashClient(newClient);
                    return;
                }

                // read command (Spectate or Announce)
                var commandMsg = GetMessage(stream, 10);
                var command    = commandMsg.TrimStart('-');
                Console.WriteLine(command);

                var isCommandValid = IsCommandValid(command);

                // Deny client for invalid command
                if ( !isCommandValid )
                {
                    TrashClient(newClient);
                    return;
                }

                // Read and verify command for the game Validate user can do command
                Guid gameId = default(Guid);

                // dont let console crash due to message not recevied as guid
                try { 
                    gameId = new Guid(GetMessage(stream, 36) );
                } catch (Exception ex)
                {
                    // todo: log this
                    Console.WriteLine("FAILED TO PARSE GAME GUID MSG!");
                    TrashClient(newClient);
                    return;

                }

                var canHandleCommand = await CanCommandBeHandled(command, apiKeyMsg, gameId);
                var validCommandMsg  = canHandleCommand ? "YES" : "NO";

                Console.WriteLine("Command valid for game? " + validCommandMsg );
                Console.WriteLine();

                // Deny client for not being allowed to spectate/announce the game
                if ( !canHandleCommand )
                {
                    TrashClient(newClient);
                    return;
                }

                // Read and verify command for the game Validate user can do command
                Guid userId = default(Guid);

                // dont let console crash due to message not recevied as guid
                try
                {
                    userId = new Guid(GetMessage(stream, 36));
                }
                catch (Exception ex)
                {
                    // todo: log this
                    Console.WriteLine("FAILED TO PARSE USER GUID MSG!");
                    TrashClient(newClient);
                    return;

                }

                // set client to appropriate command
                AddClient(command, newClient, gameId, userId);
            }
            
            // todo make this an object that is passed rather than prams
            private static void AddClient(string command, TcpClient client, Guid gameId, Guid userId)
            {
                if (command == "SPECTATE")
                {
                    lock (gameAnnouncers)
                    {
                        var newSpec = new Spectator(client, userId);

                        // only add if game being announced
                        if (gameAnnouncers.ContainsKey(gameId))
                        {
                            var announcer = gameAnnouncers[gameId];

                            // remove lingering spectator, may need to do a proper reconnect
                            announcer.spectators.RemoveWhere(spec => spec.userId == userId);
                            announcer.spectators.Add(newSpec);

                            Console.WriteLine("Added Spec!");
                        }
                    }
                }
                else if (command == "ANNOUNCE")
                {
                    lock (gameAnnouncers)
                    {
                        var newAnnouncer = new Announcer(client, userId);
                        
                        //Add if game isn't being announced
                        if (!gameAnnouncers.ContainsKey(gameId))
                        {
                            gameAnnouncers.Add(gameId, newAnnouncer);
                            Console.WriteLine("Added Announcer!");
                        }
                        // todo: properly close down announcer
                        else
                        {
                            gameAnnouncers.Remove(gameId);
                            gameAnnouncers.Add(gameId, newAnnouncer);
                            Console.WriteLine("Added Announcer!");
                        }
                    }
                }
            }

            private static bool IsCommandValid(string command)
            {
                return commands.Contains(command);
            }

            // todo: need some sort of generic request class
            private static async Task<bool> CheckValidApiKey(string command, string token)
            {
                // todo: read from file
                var apiPath  = "http://192.168.1.101:53740/api/appusers/" + command;                
                var isValid  = false;

                using ( var request = new HttpRequestMessage(HttpMethod.Get, apiPath) )
                using ( var client  = new HttpClient() )
                {
                    client.DefaultRequestHeaders.Authorization   = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response                 = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        isValid = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
                    }
                }

                return isValid;
            }

            private static async Task<bool> CanCommandBeHandled(string command, string token, Guid gameId)
            {
                // todo: read from file
                var apiPath = "http://192.168.1.101:53740/api/games/" + gameId + "/" + command;
                var isValid = false;

                using (var request = new HttpRequestMessage(HttpMethod.Get, apiPath))
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization  = new AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response                = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        isValid = JsonConvert.DeserializeObject<bool>(await response.Content.ReadAsStringAsync());
                    }
                }

                return isValid;
            }

            private static void TrashClient(TcpClient client)
            {
                client.GetStream().Close();
                client.Close();
            }         
        }

        class TcpHelper
        {
            private static TcpListener listener { get; set; }
            private static bool accept { get; set; } = false;
            // todo: read this from file
            private static string ipAddress = "192.168.1.101";

            public static void StartServer()
            {
                IPAddress address = IPAddress.Parse(ipAddress);
                listener = new TcpListener(address, 45000);

                listener.Start();
                accept = true;

                Console.WriteLine($"Server started. Listening to TCP clients at {ipAddress}");

                Listen();
            }

            public static void Listen()
            {
                if (listener != null && accept)
                {
                    // Continue listening.  
                    while (true)
                    {
                        Console.WriteLine("Waiting for client...");
                        var clientTask = listener.AcceptTcpClientAsync(); // Get the client  

                        if (clientTask.Result != null)
                        {
                            Console.WriteLine("Client attempting to connect...");
                            Thread handleClient = new Thread(() => ClientValidator.ValidateClient(clientTask.Result));
                            handleClient.Start();
                        }
                    }
                }
            }
        }

        class Announcer
        {
            Guid userId;
            string publicKey;
            TcpClient client;
            public HashSet<Spectator> spectators = new HashSet<Spectator>();

            public Announcer(TcpClient newClient, Guid newUserGuid)
            {
                client = newClient;
                userId = newUserGuid;

                Thread handleAnnounce = new Thread(() => AnnounceGameUpdate());
                handleAnnounce.Start(); // todo: store thread ?
            }

            public Task AnnounceGameUpdate()
            { 
                var lastMsg = string.Empty;

                while (lastMsg != "-------END")
                {
                    var stream  = client.GetStream();
                    lastMsg     = GetMessage(stream, 7);

                    Console.WriteLine("Message received: " + lastMsg);

                    // read game update size
                    var gameSizeMsg = lastMsg;
                    var gameSize    = Convert.ToInt32(gameSizeMsg);

                    Console.WriteLine("Game Update Size: " + gameSize + "B");
                    Console.WriteLine();

                    // read game update
                    var gameUpdate = GetMessage(stream, gameSize);
                    // todo: decrypt it

                    foreach (var spectator in spectators)
                    {
                        spectator.SendGameUpdate(gameUpdate);
                    }
                }

                Console.WriteLine("Task finished...");
                return Task.FromResult(1);
            }

            private string GetMessage(NetworkStream stream, int size)
            {
                if (stream == null) throw new ArgumentNullException("stream");

                var bytesRead = 0;
                var message = new byte[size];

                while (bytesRead < size)
                {
                    bytesRead += stream.Read(message, bytesRead, size - bytesRead);
                }

                var messageRecieved = Encoding.ASCII.GetString(message);
                return messageRecieved;
            }

        }

        class Spectator
        {
            public Guid userId { get; private set; }
            string publicKey;
            TcpClient client;

            public Spectator() { }

            public Spectator(TcpClient client, Guid userGuid)
            {
                this.client = client;
                this.userId = userGuid;
            }

            public void TriggerGameUpdate(string gameUpdate)
            {
                Thread sendUpdate = new Thread(() => SendGameUpdate(gameUpdate));
                sendUpdate.Start(); 
            }
        

            public Task SendGameUpdate(string gameUpdate)
            {
                var stream = client.GetStream();
                // todo: encrypt message
                var gameUpdateSize      = gameUpdate.Length.ToString().PadLeft(7, '0');
                var gameUpdateSizeBytes = Encoding.ASCII.GetBytes(gameUpdateSize);
                var updateBytes         = Encoding.ASCII.GetBytes(gameUpdate);

                stream.Write(gameUpdateSizeBytes);
                stream.Write(updateBytes);

                Console.WriteLine("Send game update finished...");
                return Task.FromResult(1);
            }

        }
    }
}
