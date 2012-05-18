using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc.Contracts
{
    [DataContract]
    internal class SendTransactionRpcResponse
    {
        [DataMember(Name = "sent")]
        public bool sent { get; set; }
    }
}
