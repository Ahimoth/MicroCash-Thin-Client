using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroCash.Client.Thin
{
    internal static class GlobalSettings
    {
        public static string RpcUrl { get { return "http://" + RpcAddress; } }
        public static string RpcAddress { get; set; }
        public static int ConnectionType { get; set; }
    }
}
