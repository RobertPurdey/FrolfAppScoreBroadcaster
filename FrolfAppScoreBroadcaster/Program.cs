using FrolfAppScoreBroadcaster._01._Server;
using System;
using System.Threading;

namespace TcpListenerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Broadcaster starting...");

            Thread listeningThread = new Thread(Broadcaster.StartServer);
            listeningThread.Start();
        }
    }
}
