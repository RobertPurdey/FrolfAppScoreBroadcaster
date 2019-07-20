using FrolfAppScoreBroadcaster.Server;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FrolfAppScoreBroadcaster._01._Server
{
    public class Broadcaster
    {
        private static TcpListener listener { get; set; }
        private static bool accept { get; set; } = false;

        public static void StartServer()
        {
            var serverConfig  = ReadServerConfiguration();

            IPAddress address = IPAddress.Parse(serverConfig.Ip);
            listener          = new TcpListener(address, serverConfig.Port);

            listener.Start();
            accept = true;

            Console.WriteLine($"Server started. Listening to TCP clients on {serverConfig.Ip}:{serverConfig.Port}");

            Listen();
        }

        private static ServerConfiguration ReadServerConfiguration()
        {
            using (StreamReader reader = new StreamReader("server_config.json") )
            {
                var serverJson   = reader.ReadToEnd();
                var serverConfig = JsonConvert.DeserializeObject<ServerConfiguration>(serverJson);

                return serverConfig;
            }
        }

        public static void Listen()
        {
            if (listener != null && accept)
            {
                // Continue listening.  
                while (true)
                {
                    try
                    { 
                        Console.WriteLine("Waiting for client...");
                        var clientTask = listener.AcceptTcpClientAsync(); // Get the client  

                        if (clientTask.Result != null)
                        {
                            Console.WriteLine("Client attempting to connect...");
                            Thread handleClient = new Thread(() => ClientHandler.Handle(clientTask.Result));
                            handleClient.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("==================================================================================");
                        Console.WriteLine("-----------");
                        Console.WriteLine("-- ERROR --");
                        Console.WriteLine("-----------");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("==================================================================================");
                    }
                }
            }
        }
    }
}
