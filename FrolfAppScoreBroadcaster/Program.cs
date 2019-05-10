using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        static Dictionary<Guid, HashSet<Spectator>> gameViewings = new Dictionary<Guid, HashSet<Spectator>>();

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
                    var isValid     = await CheckValidAsync("api/appusers/allowedBroadcastAccess", apiKeyMsg);
                    var validMsg    = isValid ? "YES" : "NO";

                    Console.WriteLine("Token valid? " + validMsg);
                    Console.WriteLine();

                    // read command (Spectate or Announce)
                    var commandMsg = GetMessage(stream, 10);
                    var command    = commandMsg.TrimStart('-');
                    Console.WriteLine(command);

                    // make api call to validate the user can spectate/announce token
                    // todo: validate command (use enum)?
                    //var isValid     = await CheckValidAsync("allowedBroadcastAccess", apiKeyMsg);
                    //var validMsg    = isValid ? "VALID" : "NOT VALID";

                   // Console.WriteLine(command + " command " + validMsg);
                    //Console.WriteLine();

                    // todo: we wont have to do this dumb shit anymore (using api)-- validate user exists
                    Console.WriteLine("ID found: " + RetrieveRequesterEntity());
                    Console.WriteLine();
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
            
            private static async Task<bool> CheckValidAsync(string command, string token)
            {
                var apiPath  = "http://192.168.1.101:53740/" + command;                
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
            Guid userId;
            string publicKey;
            TcpClient client;

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
