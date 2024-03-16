using System;
using System.Net;
using System.Threading;

namespace Anotarity.Scanners
{
    public class Scanner
    {
        public Int32 Threads = 16, PingTimeOut = 1200, PortTimeOut = 2400;

        IPAddress Start, End;
        Int32[] Ports = new Int32[] { 80, 8080 };
        Int32 Progress = 0;
        Thread ScanThread;
        Device[] Output = new Device[0];
        Boolean HasFinishedBool = false;

        public Scanner(IPAddress Start, IPAddress End)
        {
            this.Start = Start;
            this.End = End;
        }

        public void SetPorts(Int32[] Ports)
        {
            this.Ports = Ports;
        }

        public void Begin()
        {
            ScanThread = new Thread(ScanVoid);
            ScanThread.Start();
        }

        public Boolean HasFinished()
        {
            return HasFinishedBool;
        }

        public void Abort()
        {
            if (ScanThread != null && ScanThread.IsAlive) ScanThread.Abort();
        }

        public Device[] GetOutput()
        {
            return Output;
        }

        public int GetProgress()
        {
            return Progress;
        }

        private void ScanVoid()
        {
            IPScanner isc = new IPScanner(this.Start, this.End);
            isc.timeout = PingTimeOut;
            isc.StartScan(Threads);
            while (!isc.HasFinished()) { Progress = (int)(isc.GetProgress() * 40); Thread.Sleep(100); }
            Progress = 40;
            PortScanner psc = new PortScanner(isc.GetResult(), Ports);
            psc.timeout = PortTimeOut;
            psc.StartScan(Threads);
            while (!psc.HasFinished()) { Progress = (int)(psc.GetProgress() * 40) + 40; Thread.Sleep(100); }
            Progress = 80;
            DeviceScanner dsc = new DeviceScanner(psc.GetResult());
            dsc.StartScan(1);
            while (!dsc.HasFinished()) { Progress = (int)(dsc.GetProgress() * 20) + 80; Thread.Sleep(100); }
            Progress = 100;
            this.Output = dsc.GetResult();
            HasFinishedBool = true;
        }
    }
}
