using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Wiki.Service.Common.Clients
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceHttpClient:HttpClient
    {
        internal const string HttpLocalhost = "http://localhost";
        private readonly ServiceHttpMessageHandler _handler;


        internal ServiceHttpClient(ServiceHttpMessageHandler handler):base(handler,false)
        {
            this._handler = handler;
            //this.BaseAddress = new Uri(handler.ServiceUrl);
            this.BaseAddress=new Uri(HttpLocalhost);
        }


    }
}
