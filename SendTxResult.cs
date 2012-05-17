using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroCash.Client.Thin
{
    internal class SendTxResult
    {
        public bool IsSent { get; set; }
        public string ErrorMessage { get; set; }
    }
}
