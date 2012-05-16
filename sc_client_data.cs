/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright an licensing details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MicroCashClient
{
    #region sc_error
    public class SCErrorResponse
    {
        public SCErrorResponse()    { }
        [DataMember(Name = "result")]
        public object code { get; set; }
        [DataMember(Name = "error")]
        public SCError error { get; set; }
        [DataMember(Name = "id")]
        public int id { get; set; }
    }
    public class SCError
    {
        public SCError()    { }
        [DataMember(Name = "code")]
        public string code { get; set; }
        [DataMember(Name = "message")]
        public string message { get; set; }
    }


    #endregion

    #region sc_sendtransaction

    public class SendTransaction
    {
        public SendTransaction()
        {

        }

        [DataMember(Name = "sent")]
        public bool sent { get; set; }
    }
    

    #endregion

    #region sc_GetInfo
    /* sc_getinfo Response
    {
    "version" : 204100,
    "blocks" : 98140,
    "transactions" : 173491,
    "addresses" : 114087,
    "MicroCashs_created" : 36831860477,
    "MicroCashs_feespaid" : 655200706,
    "connections" : 1,
    "difficulty" : 30501.85912932,
    "testnet" : false,
    "mintxfee" : "500"
    }
    */
    [DataContract]
    public class GetInfo
    {
        public GetInfo()
        {
        }

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
    #endregion

    #region sc_GetWork
    [DataContract]
    public class GetWork
    {
        [DataMember(Name = "data")]
        public string Data { get; set; } //"01000000e482b2d6e6a4492b2126641c04884119271383e50337b7927f641dc7320000007b0ca180a1e7f84c81f39eb3ad8e6e1068fbf47b2b6f11a338f39f2e93e882fcc30900000000000017f3844e000000000000000000000000a7a20000000000000000000000000000000000004168696d6f74680000000000a56d3c1d";
        [DataMember(Name = "target_share")]
        public string TargetShare { get; set; } //"0x0000003c6da50000000000000000000000000000000000000000000000000000";
        [DataMember(Name = "target_real")]
        public string TargetReal { get; set; } //"0x0000003c6da50000000000000000000000000000000000000000000000000000"}
    }
    #endregion

    #region sc_GetBlockByNumber
    [DataContract]
    public class BlockHeader
    {
        public BlockHeader()
        {
        }
        [DataMember(Name = "hash")]
        public string Hash { get; set; }
        [DataMember(Name = "version")]
        public uint Version { get; set; }
        [DataMember(Name = "prev_block")]
        public string PreviousBlockHash { get; set; }
        [DataMember(Name = "mrkl_root")]
        public string MerkleRoot { get; set; }
        [DataMember(Name = "time")]
        public ulong Time { get; set; }
        [DataMember(Name = "bits")]
        public ulong Bits { get; set; }
        [DataMember(Name = "blocknum")]
        public ulong BlockNumber { get; set; }
        [DataMember(Name = "nonce1")]
        public ulong Nonce1 { get; set; }
        [DataMember(Name = "nonce2")]
        public ulong Nonce2 { get; set; }
        [DataMember(Name = "nonce3")]
        public ulong Nonce3 { get; set; }
        [DataMember(Name = "nonce4")]
        public uint Nonce4 { get; set; }
        [DataMember(Name = "miner_id")]
        public string MinerId { get; set; }
        [DataMember(Name = "miner_idhex")]
        public string MinerIdHex { get; set; }
        [DataMember(Name = "n_tx")]
        public ulong TransactionCount { get; set; }
        [DataMember(Name = "size")]
        public ulong Size { get; set; }

        [DataMember(Name = "tx")]
        public List<BlockTransaction> Transactions { get; set; }
    }

    [DataContract]
    public class BlockTransaction
    {
        public BlockTransaction()
        {
        }

        [DataMember(Name = "hash")]
        public string Hash { get; set; }
        [DataMember(Name = "version")]
        public uint Version { get; set; }
        [DataMember(Name = "lock_time")]
        public ulong LockTime { get; set; }
        [DataMember(Name = "size")]
        public ulong Size { get; set; }

        [DataMember(Name = "in")]
        public List<BlockInputTransaction> Inputs { get; set; }

        [DataMember(Name = "out")]
        public List<BlockOutputTransaction> Outputs { get; set; }

    }

    [DataContract]
    public class BlockInputTransaction
    {
        public BlockInputTransaction()
        {
        }

        [DataMember(Name = "prev_out")]
        public TransactionPreviousOutput PreviousOutput { get; set; }
        [DataMember(Name = "coinbase")]
        public string CoinBase { get; set; }
    }

    [DataContract]
    public class TransactionPreviousOutput
    {
        public TransactionPreviousOutput()
        {
        }

        [DataMember(Name = "hash")]
        public string Hash { get; set; }
        [DataMember(Name = "n")]
        public uint Number { get; set; }
    }

    [DataContract]
    public class BlockOutputTransaction
    {
        public BlockOutputTransaction()
        {
        }

        [DataMember(Name = "value")]
        public double Value { get; set; }
        [DataMember(Name = "scriptPubKey")]
        public string ScriptPublicKey { get; set; }
    }
    #endregion

    #region sc_GetBalance
    //{
    //"balance" : [
    //    {
    //        "address" : "sUKjoByAXV2HMELEjFvq9bCpnJ3g4inJJb",
    //        "balance" : 795706130,
    //        "tx" : 30
    //    }
    //]
    //}

    [DataContract]
    public class GetBalance
    {
        public GetBalance()
        {
        }

        [DataMember(Name = "balance")]
        public List<AddressBalance> AddressBalances { get; set; }

    }

    [DataContract]
    public class AddressBalance
    {
        public AddressBalance()
        {
        }

        [DataMember(Name = "address")]
        public string Address { get; set; }
        [DataMember(Name = "addressid")]
        public uint AddressID { get; set; }
        [DataMember(Name = "balance")]
        public Int64 Balance { get; set; }
        [DataMember(Name = "tx")]
        public int TxCount { get; set; }
    }
    #endregion

    #region sc_getinputs
    
    //{ "sSj4DdD5MExLgGBXrHxrNEr9WCfzBd2gx9" : [
      //{
          //"hash" : "23bf01f30829c3bc7d37fc66f3b73cb188ca7790e08aaadeae0c36bf303d5c8a",
          //"index" : 1,
          //"script" : "OP_DUP OP_HASH160 5cfc12ebe52eb8cde32948b1eadf960eeb3b9dd5 OP_EQUALVERIFY OP_CHECKSIG",
          //"value" : 5000
      //} }

    [DataContract]
    public class GetInputs
    {
        public GetInputs()
        {
        }

        [DataMember()]
        public List<InputInfo> Inputs { get; set; }

    }

    [Serializable()]
    public class SendInputInfo
    {
        public SendInputInfo()
        {
        }
        [DataMember(Name = "address")]
        public string address { get; set; }
        [DataMember(Name = "amount")]
        public Int64 amount { get; set; }

    }
    [DataContract]
    public class InputInfo
    {
        public InputInfo()
        {
        }

        [DataMember(Name = "hash")]
        public string txhash { get; set; }
        [DataMember(Name = "index")]
        public int index { get; set; }
        [DataMember(Name = "script")]
        public string script { get; set; }
        [DataMember(Name = "value")]
        public Int64 amount { get; set; }        
    }

    #endregion

    #region sc_GetHistory
    //{
    //"history" : [
        //{
            //"hash" : "0x8b74b6542823353d3e90a5263cfbe84a21e554f812ce99cc53114f0c84ff4ea0",
            //"time" : 1331602976,
            //"block" : 239167,
            //"amount" : 6660,
            //"type" : 2
        //},
    //]
    //}

    [DataContract]
    public class GetHistory
    {
        public GetHistory()
        {
        }

        [DataMember(Name = "history")]
        public List<AddressHistory> AddressHistory { get; set; }

    }

    [DataContract]
    public class AddressHistory
    {
        public AddressHistory()
        {
        }

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
    #endregion
}
