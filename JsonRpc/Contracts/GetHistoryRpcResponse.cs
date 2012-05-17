using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc.Contracts
{
    [DataContract]
    internal class GetHistoryRpcResponse
    {
        [DataMember(Name = "history")]
        public List<AddressHistory> AddressHistory { get; set; }
    }

    [DataContract]
    internal class AddressHistory
    {
        public string account;
        [DataMember(Name = "hash")]
        public string hash { get; set; }
        [DataMember(Name = "time")]
        public Int64 time { get; set; }
        [DataMember(Name = "block")]
        public Int64 block { get; set; }
        [DataMember(Name = "amount")]
        public Int64 amount { get; set; }
        [DataMember(Name = "fromto")]
        public string fromto { get; set; }
        [DataMember(Name = "info")]
        public string info { get; set; }
        [DataMember(Name = "type")]
        public int type { get; set; }
    }
}
