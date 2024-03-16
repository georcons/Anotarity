using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Anotarity.Net
{
    public class SocketTools
    {
        public static bool Connect(Socket s, IPEndPoint iep, int timeout)
        {
            bool Connected = false, Finished = false;
            Thread ConnectThread, TimeOutThread;

            ConnectThread = new Thread(() =>
            {
                s.Connect(iep);
                Connected = true;
                Finished = true;
            });

            TimeOutThread = new Thread(() =>
            {
                Thread.Sleep(timeout);
                if (!Connected) ConnectThread.Abort();
                Finished = true;
            });

            ConnectThread.Start();
            TimeOutThread.Start();

            while (!Finished) Thread.Sleep(30);

            return Connected;
        }
    }
}