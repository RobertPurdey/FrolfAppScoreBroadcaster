using FrolfAppScoreBroadcaster.Server;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpListenerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Start the server  
            Thread listeningThread = new Thread(TcpHelper.StartServer);
            listeningThread.Start();
        }

        class TcpHelper
        {
            private static TcpListener listener { get; set; }
            private static bool accept { get; set; } = false;
            // todo: read this from file
            private static string ipAddress = "192.168.1.86";

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
                            Thread handleClient = new Thread(() => ClientHandler.Handle(clientTask.Result));
                            handleClient.Start();
                        }
                    }
                }
            }
        }
    }
}
