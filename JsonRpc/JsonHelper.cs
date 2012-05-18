using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Net;
using MicroCash.Client.Thin.JsonRpc.Contracts;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc
{
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
                        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(RpcErrorResponse));
                        RpcErrorResponse response;
                        try
                        {
                            response = (RpcErrorResponse)deserializer.ReadObject(sr);
                            retresponse.HttpErrorMessage = response.error.message;
                            //MessageBox.Show(response.error.message, "Server returned error");
                        }
                        catch (SerializationException)
                        {
                            //MessageBox.Show("Error retrieving from the Node.\nThe server seems to return an unexpected Dataset");
                            retresponse.HttpErrorMessage = "Unknown error";
                        }
                    }
                else if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    retresponse.HttpErrorMessage = "Unable to connect to " + url;
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
                //MessageBox.Show(ex.Message);
                return response;
            }
        }

        public static T GetObjectFromHTML<T>(string url)
        {
            try
            {
                string json = null;
                using (var webClient = new System.Net.WebClient())
                {
                    json = webClient.DownloadString(url);
                }

                //deserialize the response                	
                MemoryStream stream = new MemoryStream((Encoding.UTF8.GetBytes(json)));
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));

                stream.Position = 0;
                T response = (T)deserializer.ReadObject(stream);
                stream.Close();
                //JsonResponse<T> response = (JsonResponse<T>)JsonConvert.DeserializeObject(json);
                return response;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
