using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Anotarity.Scanners
{
    public class PortScanner
    {
        public int timeout = 2400;

        int CurrentIndex;
        IPEndPoint[] Targets;
        int NumOfScannedTargets = 0;
        bool[] ThreadPools;
        IPEndPoint[] Result = new IPEndPoint[0];
        bool Begin = false, EdittingResults = false, ThreadCallback = false;
        Thread[] Threads;
        public PortScanner(IPAddress[] IPs, int[] ports)
        {
            this.Targets = new IPEndPoint[IPs.Length * ports.Length];
            for (int i = 0; i < IPs.Length; i++)
            {
                for (int j = 0; j < ports.Length; j++) this.Targets[ports.Length * i + j] = new IPEndPoint(IPs[i], ports[j]);
            }
            this.CurrentIndex = -1;
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
                while (!ThreadCallback) Thread.Sleep(30);
            }
            Begin = true;
        }

        public double GetProgress()
        {
            return (double)NumOfScannedTargets / Targets.Length;
        }

        public bool HasFinished()
        {
            if (!Begin) return false;
            for (int i = 0; i < ThreadPools.Length; i++)
                if (!ThreadPools[i]) return false;
            return true;
        }

        public IPEndPoint[] GetResult()
        {
            IPEndPoint[] Output = new IPEndPoint[Result.Length];
            for (int i = 0; i < Result.Length; i++) Output[i] = Result[i];
            return Output;
        }

        private void ThreadMethod(int ind)
        {
            ThreadCallback = true;
            int index = ind;
            while (!Begin) Thread.Sleep(100);
            int TargetIndex;
            while ((TargetIndex = NextTargetIndex()) < Targets.Length)
            {
                IPEndPoint Target = Targets[TargetIndex];
                if (IsPortOpen(Target, timeout)) AddResult(Target);
                NumOfScannedTargets++;
                Thread.Sleep(20);
            }
            ThreadPools[index] = true;
        }

        private void AddResult(IPEndPoint iep)
        {
            while (EdittingResults) Thread.Sleep(1);
            EdittingResults = true;
            IPEndPoint[] NewResult = new IPEndPoint[Result.Length + 1];
            for (int i = 0; i < Result.Length; i++) NewResult[i] = Result[i];
            NewResult[Result.Length] = iep;
            Result = NewResult;
            EdittingResults = false;
        }

        private int NextTargetIndex()
        {
            return ++CurrentIndex;
        }

        private bool IsPortOpen(IPEndPoint iep, int timeout)
        {
            Boolean Finished = false, Result = false;
            TcpClient client = new TcpClient();

            Thread ConnectThread = new Thread(() =>
            {
                try
                {
                    client.Connect(iep);
                    Result = true;
                    Finished = true;
                }
                catch { Finished = true; }
            });

            Thread ControlThread = new Thread(() =>
            {
                Thread.Sleep(timeout);
                Finished = true;
            });

            ConnectThread.Start();
            ControlThread.Start();

            while (!Finished) Thread.Sleep(20);

            client.Close();
            ConnectThread.Abort();
            ControlThread.Abort();


            return Result;
        }
    }
}