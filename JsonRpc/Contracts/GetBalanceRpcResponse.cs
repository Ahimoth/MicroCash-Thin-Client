using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc.Contracts
{
    [DataContract]
    internal class GetBalanceRpcResponse
    {
        [DataMember(Name = "balance")]
        public List<AddressBalance> AddressBalances { get; set; }
    }

    [DataContract]
    internal class AddressBalance
    {
        [DataMember(Name = "address")]
        public string Address { get; set; }
        [DataMember(Name = "addressid")]
        public uint AddressID { get; set; }
        [DataMember(Name = "balance")]
        public Int64 Balance { get; set; }
        [DataMember(Name = "tx")]
        public int TxCount { get; set; }
    }
}

