using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MicroCash.Client.Thin.JsonRpc
{
    [DataContract]
    internal class JsonResponse<T>
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
