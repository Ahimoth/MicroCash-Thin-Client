using System;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc.Contracts
{
    [DataContract]
    internal class GetInfoRpcResponse
    {
        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "blocks")]
        public Int64 Blocks { get; set; }

        [DataMember(Name = "transactions")]
        public Int64 Transactions { get; set; }

        [DataMember(Name = "accounts")]
        public Int64 Addresses { get; set; }

        [DataMember(Name = "MicroCash_created")]
        public Int64 MicroCashs_created { get; set; }

        [DataMember(Name = "MicroCash_feespaid")]
        public Int64 MicroCashs_feespaid { get; set; }

        [DataMember(Name = "connections")]
        public int Connections { get; set; }

        [DataMember(Name = "difficulty")]
        public double Difficulty { get; set; }

        [DataMember(Name = "network_hashrate")]
        public Int64 MicroCashs_networkhashrate { get; set; }

        [DataMember(Name = "testnet")]
        public bool TestNet { get; set; }

        [DataMember(Name = "mintxfee")]
        public double MinTxFee { get; set; }
    }
}
