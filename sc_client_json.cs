/*
 * MicroCash Thin Client
 * Please see License.txt for applicable copyright an licensing details.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Configuration;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace MicroCashClient
{
    public class SC_Transaction
    {
        public uint m_dwType;
        public uint m_dwAddressID;
        public byte[] m_FromAddress;
        public byte[] m_RecvAddress;
        public byte[] m_Info;        
        public Int64 m_qAmount;
        public byte[] m_Signature;
        public byte[] m_Extra1;
        public byte[] m_Extra2;

        public SC_Transaction()
        {
            m_Info = new byte[8];
            m_FromAddress = new byte[10];
            m_RecvAddress = new byte[10];
            m_Signature = new byte[64];
            m_Extra1 = new byte[64];
            m_Extra2 = new byte[64];
        }

        public byte[] GetByteBuffer(bool bIncludeSig)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(m_dwType);
            bw.Write(m_dwAddressID);
            bw.Write(m_FromAddress);
            bw.Write(m_RecvAddress);
            bw.Write(m_Info);            
            bw.Write(m_qAmount);
            if (m_dwType != 0)            
            {
                bw.Write(m_Extra1);
                bw.Write(m_Extra2);
            }
            if(bIncludeSig) bw.Write(m_Signature);
            return ms.ToArray();            
        }

        public byte[] GetHash(bool bIncludeSig)
        {
            
            SHA256 shaM = new SHA256Managed();
            byte[] result = shaM.ComputeHash(GetByteBuffer(bIncludeSig));
            byte[] result2 = shaM.ComputeHash(result);
            return result2;
        }
             

        
    }

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    };

    static class MicroCashFunctions
    {
        public static string ToHex(byte[] ba)
        {
            StringBuilder sb = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba) sb.AppendFormat("{0:x2}", b);
            return sb.ToString();
        }

        public static string MoneyToString(Int64 money)
        {
            return (money / 10000.0).ToString();
        }
    }

    public class MicroCashRPC
    {
        public string m_RPCurl;
        public string m_ErrorMessage;

        public MicroCashRPC(string url)
        {
            m_RPCurl = url;
        }

        public BlockHeader GetBlockData(long blockNumber)
        {
            object[] parameters = { blockNumber };
            JsonResponse<BlockHeader> response = JsonHelper.GetObjectFromJsonRPC<BlockHeader>(m_RPCurl, "sc_getblockbynumber", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        public GetInfo GetInfo()
        {
            object[] parameters = null;
            JsonResponse<GetInfo> response = JsonHelper.GetObjectFromJsonRPC<GetInfo>(m_RPCurl, "sc_getinfo", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        public GetBalance GetBalance(List<string> addresses)
        {
            object[] parameters = new object[addresses.Count];
            for (int x = 0; x < addresses.Count; x++) parameters[x] = addresses[x];

            JsonResponse<GetBalance> response = JsonHelper.GetObjectFromJsonRPC<GetBalance>(m_RPCurl, "sc_getbalance", "thin","client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        public GetHistory GetHistory(string address, int nTransactions)
        {
            object[] parameters = { address, nTransactions };
            JsonResponse<GetHistory> response = JsonHelper.GetObjectFromJsonRPC<GetHistory>(m_RPCurl, "sc_gethistory", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        public SendTransaction SendTransaction(string hexbytes)
        {
            object[] parameters = { hexbytes };
            JsonResponse<SendTransaction> response = JsonHelper.GetObjectFromJsonRPC<SendTransaction>(m_RPCurl, "sc_sendtransaction", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }

        public GetInfo GetTransactions()
        {
            object[] parameters = null;
            JsonResponse<GetInfo> response = JsonHelper.GetObjectFromJsonRPC<GetInfo>(m_RPCurl, "sc_getinfo", "thin", "client", parameters);
            m_ErrorMessage = response.HttpErrorMessage;
            return response.Result;
        }
    }

    public class MyObject 
    {
        public string Id {get;set;}
        public string Text {get;set;}
    }


    internal static class JsonHelper
    {


        public static JsonResponse<T> GetObjectFromJsonRPC<T>(string url, string command, object[] parameters)
        {
            return GetObjectFromJsonRPC<T>(url, command, null, null, parameters);
        }

        public static JsonResponse<T> GetObjectFromJsonRPC<T>(string url, string command, string user, string pass, object[] parameters)
        {
            JsonCommand jsonCmd = new JsonCommand(command, parameters);
            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);

                //we need to do a POST for json
                webRequest.Method = "POST";

                //set the content type to json
                webRequest.ContentType = "application/json; charset=utf-8";

                //if credentials were supplied, use them
                if (user != null)
                    webRequest.Credentials = new NetworkCredential(user, pass);

                //write the json data to the request (post) stream
                using (Stream s = webRequest.GetRequestStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(JsonCommand));
                    serializer.WriteObject(s, jsonCmd);
                }

                //read the json data from the response stream
                using (Stream s = webRequest.GetResponse().GetResponseStream())
                {
                    //deserialize the response
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(JsonResponse<T>));
                    JsonResponse<T> response = (JsonResponse<T>)deserializer.ReadObject(s);
                    response.HttpErrorMessage = "";
                    return response;
                }
            }
            catch (WebException ex)
            {

                JsonResponse<T> retresponse = new JsonResponse<T>(jsonCmd.Id);
                retresponse.HttpErrorMessage = "";
                if (ex.Status == WebExceptionStatus.ProtocolError)
                    using (Stream sr = ex.Response.GetResponseStream())
                    {
                        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(SCErrorResponse));
                        SCErrorResponse response = (SCErrorResponse)deserializer.ReadObject(sr);
                        retresponse.HttpErrorMessage = response.error.message;
                        //MessageBox.Show(response.error.message, "Server returned error");
                    }
                else if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    retresponse.HttpErrorMessage = "Unable to connect to "+url;
                }
                else
                {
                    retresponse.HttpErrorMessage = ex.Message;
                }

                return retresponse;
            }
            catch (Exception ex)
            {
                JsonResponse<T> response = new JsonResponse<T>(jsonCmd.Id);
                response.FrameworkErrorMessage = ex.Message;
                MessageBox.Show(ex.Message);
                return response;
            }
        }

        

        public static Object GetObjectFromHTML<t>(string url)
        {            
            try
            {
                string json = null;
                using (var webClient = new System.Net.WebClient())
                {
                    json = webClient.DownloadString(url);
                    // Now parse with JSON.Net
                }
                                
                  
                //deserialize the response                	
                MemoryStream stream = new MemoryStream((Encoding.UTF8.GetBytes(json)));
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(t));

                stream.Position = 0;
                Object response = (t)deserializer.ReadObject(stream);
                stream.Close();
                //JsonResponse<T> response = (JsonResponse<T>)JsonConvert.DeserializeObject(json);
                return response;
            }           
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                return null;
            }
        }



    }
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

    [DataContract]
    public class JsonResponse<T>
    {
        public JsonResponse()
        {
        }

        public JsonResponse(string id)
        {
            Id = id;
        }

        public JsonResponse(string id, string error, T result)
        {
            Result = result;
            Id = id;
            Error = error;
        }

        [DataMember(Name = "result")]
        public T Result { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        public string HttpResponseCode { get; set; }

        public string HttpErrorMessage { get; set; }

        public string FrameworkErrorMessage { get; set; }
    }
}
