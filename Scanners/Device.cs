using System;
using System.Net;

using Anotarity.Models;
using Anotarity.Profiles;
using Anotarity.Exploits;

namespace Anotarity.Scanners
{
    public class Device
    {
        public IPEndPoint EndPoint = new IPEndPoint(IPAddress.None, 0);
        public Model Model = Model.None;
        public Boolean Exploited = false;
        public ExploitType[] Exploits;
        public Profile[] Profiles;
    }
}
