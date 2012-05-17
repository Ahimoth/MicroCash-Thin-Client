using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc.Contracts
{
    [DataContract]
    internal class RpcErrorResponse
    {
        public RpcErrorResponse() { }
        [DataMember(Name = "result")]
        public object code { get; set; }
        [DataMember(Name = "error")]
        public RpcError error { get; set; }
        [DataMember(Name = "id")]
        public int id { get; set; }
    }

    [DataContract]
    internal class RpcError
    {
        public RpcError() { }
        [DataMember(Name = "code")]
        public string code { get; set; }
        [DataMember(Name = "message")]
        public string message { get; set; }
    }
}
