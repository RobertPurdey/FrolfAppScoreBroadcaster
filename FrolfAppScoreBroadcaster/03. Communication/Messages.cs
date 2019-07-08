using System;
using System.Net.Sockets;
using System.Text;

namespace FrolfAppScoreBroadcaster._03._Communication
{
    public class Messages
    {
        public static string GetMessage(NetworkStream stream, int size)
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
    }
}
