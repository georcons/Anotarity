using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

using Anotarity.Models;
using Anotarity.Exploits;

namespace Anotarity.Scanners
{
    public class DeviceScanner
    {
        public int timeout = 2400;

        int CurrentIndex;
        IPEndPoint[] Targets;
        int NumOfScannedTargets = 0;
        bool[] ThreadPools;
        Device[] Result = new Device[0];
        bool Begin = false, EdittingResults = false, ThreadCallback = false;
        Thread[] Threads;
        public DeviceScanner(IPEndPoint[] Targets)
        {
            this.Targets = Targets;
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
                while (!ThreadCallback) Thread.Sleep(4);
            }
            Begin = true;
        }

        public double GetProgress()
        {
            return (double)NumOfScannedTargets / (double)Targets.Length;
        }

        public bool HasFinished()
        {
            if (!Begin) return false;
            for (int i = 0; i < ThreadPools.Length; i++)
                if (!ThreadPools[i]) return false;
            return true;
        }

        public Device[] GetResult()
        {
            Device[] Output = new Device[Result.Length];
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
                ExploitScanner Scanner = new ExploitScanner(Targets[TargetIndex]);
                Scanner.Scan(true);
                if (Scanner.DeviceModel != Model.None)
                {
                    Device NewDevice = new Device();
                    NewDevice.EndPoint = Targets[TargetIndex];
                    NewDevice.Model = Scanner.DeviceModel;
                    NewDevice.Exploited = Scanner.Exploits.Length != 0;
                    NewDevice.Exploits = Scanner.Exploits;
                    NewDevice.Profiles = Scanner.Profiles;
                    AddResult(NewDevice);
                }
                NumOfScannedTargets++;
            }
            ThreadPools[index] = true;
        }

        private void AddResult(Device device)
        {
            while (EdittingResults) continue;
            EdittingResults = true;
            Device[] NewResult = new Device[Result.Length + 1];
            for (int i = 0; i < Result.Length; i++) NewResult[i] = Result[i];
            NewResult[Result.Length] = device;
            Result = NewResult;
            EdittingResults = false;
        }

        private int NextTargetIndex()
        {
            return ++CurrentIndex;
        }
    }
}
