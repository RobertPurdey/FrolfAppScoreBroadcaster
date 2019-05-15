using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace TcpListenerTest
{
    class Program
    {
        static Dictionary<Guid, HashSet<Spectator>> gameSpectators = new Dictionary<Guid, HashSet<Spectator>>();
        static Dictionary<Guid, HashSet<Announcer>> gameAnnouncers = new Dictionary<Guid, HashSet<Announcer>>();

        static HashSet<string> commands = new HashSet<string>() { "SPECTATE", "ANNOUNNCE" };


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // threads i need
            //  - listening for connections
            //  - accepting connections
            //  - announcing game


            // Start the server  
            Thread listeningThread = new Thread(TcpHelper.StartServer);
            listeningThread.Start();
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

                using (var stream = newClient.GetStream())
                {
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

                    // make api call to validate the user can spectate/announce token
                    // todo: validate command (use enum)?
                    //var isValid     = await CheckValidAsync("allowedBroadcastAccess", apiKeyMsg);
                    //var validMsg    = isValid ? "VALID" : "NOT VALID";

                    // Console.WriteLine(command + " command " + validMsg);
                    //Console.WriteLine();
                }

                // read API keydi
                // decrypt api key
                // validate user exists
                // read command (Spectate or Announce)
                // if Spectate
                // if can spectate
                // add to spectate list if it is being announced, otherwise close con
                // close con if cant spectate

                // if Announce
                // if can announce
                // add newly announced game to list
                // close con if cant announce
            }
            
            // todo make this an object that is passed rather than prams
            private static void AddClient(string command, TcpClient client, Guid gameId, Guid userId)
            {
                if (command == "SPECTATE")
                {
                    lock (gameSpectators)
                    {
                        var newSpec = new Spectator(client, userId);

                        if (gameSpectators.ContainsKey(gameId))
                        {
                            var specators = gameSpectators[gameId];

                            // remove lingering spectator, may need to do a proper reconnect
                            specators.RemoveWhere(spec => spec.userId == userId);
                            specators.Add(newSpec);
                        }
                        else
                        {
                            gameSpectators.Add(gameId, new HashSet<Spectator>(){ newSpec } );
                        }

                        Console.WriteLine("Added Spec!");
                    }
                }
                else if (command == "ANNOUNCE")
                {

                }
            }

            private static bool IsCommandValid(string command)
            {
                return commands.Contains(command);
            }

            // todo: need some sort of generic request class
            private static async Task<bool> CheckValidApiKey(string command, string token)
            {
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

            private static Guid RetrieveRequesterEntity()
            {
                Guid foundId = default(Guid);
                try
                {
                    SqlConnectionStringBuilder connBuilder = new SqlConnectionStringBuilder();

                    connBuilder.DataSource = "DESKTOP-AEISIBB\\SQLEXPRESS";
                    connBuilder.UserID = "sa";
                    connBuilder.Password = "tHu55er123";
                    connBuilder.InitialCatalog = "frolf.3.dev";

                    using (SqlConnection connection = new SqlConnection(connBuilder.ConnectionString))
                    {
                        connection.Open();

                        StringBuilder strBuilder = new StringBuilder();
                        strBuilder.Append("SELECT u.id as id ");
                        strBuilder.Append("FROM [dbo].[app_user] as u ");
                        strBuilder.Append("WHERE u.id = 'f9968023-583c-4962-ae94-dca322755069'");

                        string cmdText = strBuilder.ToString();

                        using (SqlCommand sqlCmd = new SqlCommand(cmdText, connection))
                        {
                            using (SqlDataReader sqlReader = sqlCmd.ExecuteReader())
                            {
                                while (sqlReader.Read())
                                {
                                    foundId = sqlReader.GetGuid(0);
                                }
                            }
                        }
                    }
                }
                catch (SqlException e)
                {
                    Console.WriteLine(e.InnerException);
                }

                return foundId;
            }

        }

        class TcpHelper
        {
            private static TcpListener listener { get; set; }
            private static bool accept { get; set; } = false;
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

                        // 1. Accepting client
                        //      - validate API key
                        //      - determine the request (Spectate game or Announce game)
                        //      - if announce
                        //          - validate user can announce the game (creator of the game)
                        //          - if can announce
                        //              - if game id is not in dictionary also spin up a new game announce thread thread
                        //              - wait for update loop
                        //                  - push it to all clients spectating the game being announced
                        //      - else if spectate
                        //          - validate user can spectate the game
                        //          - if can spectate
                        //              - add to dictionairy
                        //          - if game id is not dictionairy 
                        //              - deny user game isn't being broadcasted

                        if (clientTask.Result != null)
                        {
                            // in here send off to a new thread to validate the connection to free up listening thread
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

            public Task AnnounceGameUpdate(string gameUpdate)
            {
                int i = 0;
                i = 1 + 1;

                Console.WriteLine("Task finished...");
                return Task.FromResult(1);
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

            public Task SendGameUpdate(string gameUpdate)
            {
                int i = 0;
                i = 1 + 1;

                Console.WriteLine("Task finished...");
                return Task.FromResult(1);
            }

        }
    }
}
