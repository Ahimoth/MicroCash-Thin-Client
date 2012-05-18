using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroCash.Client.Thin
{
    internal static class MicroCashHelper
    {
        public static string ToHex(byte[] ba)
        {
            StringBuilder sb = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        public static string MoneyToString(Int64 money)
        {
            return (money / 10000.0).ToString();
        }
    }
}
