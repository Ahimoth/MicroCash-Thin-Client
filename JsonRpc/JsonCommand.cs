using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc
{
    [DataContract]
    public class JsonCommand
    {
        public JsonCommand(string jsonMethod)
        {
            JsonRpcVersion = "1.0";
            Id = "1";
            Method = jsonMethod;
            Params = null;
        }

        public JsonCommand(string jsonMethod, object[] jsonParameters)
        {
            JsonRpcVersion = "1.0";
            Id = "1";
            Method = jsonMethod;
            Params = jsonParameters;
        }

        [DataMember(Name = "params")]
        public object[] Params { get; set; }

        [DataMember(Name = "method")]
        public string Method { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "jsonrpc")]
        public string JsonRpcVersion { get; set; }

    }
}
