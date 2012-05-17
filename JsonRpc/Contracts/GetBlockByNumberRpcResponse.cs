//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.Serialization;

//namespace MicroCash.Client.Thin.JsonRpc.Contracts
//{
//    [DataContract(Name="BlockHeader")]
//    internal class GetBlockByNumberRpcResponse
//    {
//        [DataMember(Name = "hash")]
//        public string Hash { get; set; }
//        [DataMember(Name = "version")]
//        public uint Version { get; set; }
//        [DataMember(Name = "prev_block")]
//        public string PreviousBlockHash { get; set; }
//        [DataMember(Name = "mrkl_root")]
//        public string MerkleRoot { get; set; }
//        [DataMember(Name = "time")]
//        public ulong Time { get; set; }
//        [DataMember(Name = "bits")]
//        public ulong Bits { get; set; }
//        [DataMember(Name = "blocknum")]
//        public ulong BlockNumber { get; set; }
//        [DataMember(Name = "nonce1")]
//        public ulong Nonce1 { get; set; }
//        [DataMember(Name = "nonce2")]
//        public ulong Nonce2 { get; set; }
//        [DataMember(Name = "nonce3")]
//        public ulong Nonce3 { get; set; }
//        [DataMember(Name = "nonce4")]
//        public uint Nonce4 { get; set; }
//        [DataMember(Name = "miner_id")]
//        public string MinerId { get; set; }
//        [DataMember(Name = "miner_idhex")]
//        public string MinerIdHex { get; set; }
//        [DataMember(Name = "n_tx")]
//        public ulong TransactionCount { get; set; }
//        [DataMember(Name = "size")]
//        public ulong Size { get; set; }

//        [DataMember(Name = "tx")]
//        public List<BlockTransaction> Transactions { get; set; }
//    }

//    [DataContract]
//    public class BlockTransaction
//    {
//    }
//}
