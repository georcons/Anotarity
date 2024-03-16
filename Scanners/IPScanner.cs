using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;

namespace Anotarity.Scanners
{
    public class IPScanner
    {
        public int timeout = 1200, ttl = 128;

        uint StartIP, EndIP, CurrentIP;
        int NumOfScannedIPs = 0;
        bool[] ThreadPools;
        IPAddress[] Result = new IPAddress[0];
        bool Begin = false, EdittingResults = false, ThreadCallback = false;
        Thread[] Threads;
        public IPScanner(IPAddress Start, IPAddress End)
        {
            this.StartIP = IPAddressToUInt(Start);
            this.EndIP = IPAddressToUInt(End);
            this.CurrentIP = this.StartIP - 1;
        }

        public void StartScan(int ThreadCount)
        {
            this.Threads = new Thread[ThreadCount];
            this.ThreadPools = new bool[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                this.Threads[i] = new Thread(() => ThreadMethod(i));
                ThreadCallback = false;
                this.Threads[i].Start();
                while (!ThreadCallback) Thread.Sleep(4);
            }
            Begin = true;
        }

        public double GetProgress()
        {
            return (double)NumOfScannedIPs / (double)(EndIP - StartIP + 1);
        }

        public bool HasFinished()
        {
            if (!Begin) return false;
            for (int i = 0; i < ThreadPools.Length; i++)
                if (!ThreadPools[i]) return false;
            return true;
        }

        public IPAddress[] GetResult()
        {
            IPAddress[] Output = new IPAddress[Result.Length];
            for (int i = 0; i < Result.Length; i++) Output[i] = Result[i];
            return Output;
        }

        private void ThreadMethod(int ind)
        {
            ThreadCallback = true;
            int index = ind;
            while (!Begin) Thread.Sleep(100);
            uint IPToScan;
            while ((IPToScan = NextIP()) < EndIP)
            {
                IPAddress Address = UIntToIPAddress(IPToScan);
                Ping pingSender = new Ping();
                PingOptions options = new PingOptions();
                options.Ttl = ttl;
                options.DontFragment = true;
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                PingReply reply = pingSender.Send(Address, timeout, buffer, options);
                if (reply.Status == IPStatus.Success) AddIP(Address);
                NumOfScannedIPs++;
                Thread.Sleep(20);
            }
            ThreadPools[index] = true;
        }

        private void AddIP(IPAddress Address)
        {
            while (EdittingResults) continue;
            EdittingResults = true;
            IPAddress[] NewResult = new IPAddress[Result.Length + 1];
            for (int i = 0; i < Result.Length; i++) NewResult[i] = Result[i];
            NewResult[Result.Length] = Address;
            Result = NewResult;
            EdittingResults = false;
        }

        private uint NextIP()
        {
            return ++CurrentIP;
        }

        private static uint IPAddressToUInt(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();

            // flip big-endian(network order) to little-endian
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        private static IPAddress UIntToIPAddress(uint ipAddress)
        {
            byte[] bytes = BitConverter.GetBytes(ipAddress);

            // flip little-endian to big-endian(network order)
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return new IPAddress(bytes);
        }
    }
}