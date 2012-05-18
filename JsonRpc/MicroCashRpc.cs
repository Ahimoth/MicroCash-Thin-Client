using System.Collections.Generic;
using MicroCash.Client.Thin.JsonRpc.Contracts;

namespace MicroCash.Client.Thin.JsonRpc
{
    internal class MicroCashRpcClient
    {
        public string m_RPCurl;
        private string m_ErrorMessage;

        public string ErrorMessage
        {
            get { return m_ErrorMessage; }
        }

        public MicroCashRpcClient(string url)
        {
            m_RPCurl = url;
        }

        //public GetBlockByNumberRpcResponse GetBlockData(long blockNumber)
        //{
        //    object[] parameters = { blockNumber };
        //    JsonResponse<GetBlockByNumberRpcResponse> response = JsonHelper.GetObjectFromJsonRPC<GetBlockByNumberRpcResponse>(m_RPCurl, "sc_getblockbynumber", "thin", "client", parameters);
        //    m_ErrorMessage = response.HttpErrorMessage;
        //    return response.Result;
        //}

        internal GetInfoRpcResponse GetInfo()
        {
            object[] parameters = null;
            JsonResponse<GetInfoRpcResponse> response = JsonHelper.GetObjectFromJsonRPC<GetInfoRpcResponse>(m_RPCurl, "sc_getinfo", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        internal GetBalanceRpcResponse GetBalance(List<string> addresses)
        {
            object[] parameters = new object[addresses.Count];
            for (int x = 0; x < addresses.Count; x++) parameters[x] = addresses[x];

            JsonResponse<GetBalanceRpcResponse> response = JsonHelper.GetObjectFromJsonRPC<GetBalanceRpcResponse>(m_RPCurl, "sc_getbalance", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        internal GetHistoryRpcResponse GetHistory(string address, int nTransactions)
        {
            object[] parameters = { address, nTransactions };
            JsonResponse<GetHistoryRpcResponse> response = JsonHelper.GetObjectFromJsonRPC<GetHistoryRpcResponse>(m_RPCurl, "sc_gethistory", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        internal SendTransactionRpcResponse SendTransaction(string hexbytes)
        {
            object[] parameters = { hexbytes };
            JsonResponse<SendTransactionRpcResponse> response = JsonHelper.GetObjectFromJsonRPC<SendTransactionRpcResponse>(m_RPCurl, "sc_sendtransaction", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        internal GetInfoRpcResponse GetTransactions()
        {
            object[] parameters = null;
            JsonResponse<GetInfoRpcResponse> response = JsonHelper.GetObjectFromJsonRPC<GetInfoRpcResponse>(m_RPCurl, "sc_getinfo", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }
    }
}
