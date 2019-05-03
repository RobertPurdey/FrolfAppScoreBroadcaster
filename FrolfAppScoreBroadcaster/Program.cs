using System;
using System.Collections.Generic;
using System.Net;
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

            public static void ValidateClient(TcpClient newClient)
            {
                var buffer = new byte[1024];

                using (var stream = newClient.GetStream())
                {
                    // read the size of the api key being sent (the size will always be contained to 20 bytes) size is written in bytes)
                    var tokenSizeMsg = GetMessage(stream, 4);
                    var tokenSize = Convert.ToInt32(tokenSizeMsg);
                    Console.WriteLine("Token Size: " + tokenSize + "B");

                    // read API key
                    var apiKeyMsg = GetMessage(stream, tokenSize);
                    int a = 1;
                    Console.WriteLine("Token: " + apiKeyMsg);

                    // decrypt api key


                    // validate user exists


                    // read command (Spectate or Announce)
                    var commandMsg = GetMessage(stream, 10);
                    var command = commandMsg.TrimStart('-');
                    Console.WriteLine(command);
                }

                // read API key
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
        }

        class TcpHelper
        {
            private static TcpListener listener { get; set; }
            private static bool accept { get; set; } = false;

            public static void StartServer()
            {
                IPAddress address = IPAddress.Parse("192.168.1.66");
                listener = new TcpListener(address, 45000);

                listener.Start();
                accept = true;

                Console.WriteLine($"Server started. Listening to TCP clients at 192.168.1.66:45000");

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
