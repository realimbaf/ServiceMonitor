using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Wiki.Core.Exceptions;
using Wiki.Core.Extensions;

namespace Wiki.Service.Common.Clients
{
    public class ServiceHttpMessageHandler : HttpClientHandler
    {
        private readonly string _id;
        private readonly string _discoveryUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private bool _isInit;

        private string _serviceUrl;

        private object _lockObj = new object();
        private string _oldUrl;

        public ServiceHttpMessageHandler(string id, string discoveryUrl, string clientId, string clientSecret)
        {
            this._id = id;
            this._discoveryUrl = discoveryUrl;
            this._clientId = clientId;
            this._clientSecret = clientSecret;


            this._oldUrl = ServiceHttpClient.HttpLocalhost;
            Debug.WriteLine("Created handller");

        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            var task = PreareSend(request, cancellationToken)
                ;
            HttpResponseMessage httpResponseMessage;

            var t1= await task.ContinueWith(t =>
            {
                HttpRequestException error = null;
                if (t.Exception != null && t.Exception.InnerException != null)
                    error = t.Exception.InnerException as HttpRequestException;
                if(error!=null)
                {
                    this._isInit = false;
                    Debug.WriteLine("Reinit handler. ");
                    return PreareSend(request, cancellationToken);
                    
                }
                return t;
            }, cancellationToken);
            httpResponseMessage = await t1;
            return httpResponseMessage;
        }

        private async Task<HttpResponseMessage> PreareSend(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Init();

            PrepareUrl(request);
            var cl = new ServiceMonitorClient(this._discoveryUrl, this._clientId, this._clientSecret);
            var token = await cl.GetTokenAsync(false);

            request.Headers.Authorization=new AuthenticationHeaderValue(token.TokenType,token.Token);

            var result = await base.SendAsync(request, cancellationToken);
            return result;
        }



        private void PrepareUrl(HttpRequestMessage request)
        {
            var url = new Uri(new Uri(this._serviceUrl),request.RequestUri.PathAndQuery);

            Debug.WriteLine(string.Format("PrepareUrl request:{2}{0} service:{1}{0}new:{3}", Environment.NewLine, this._serviceUrl, request.RequestUri.AbsoluteUri, url));

            var absoluteUri = request.RequestUri.AbsoluteUri;

            //this._oldUrl = this._serviceUrl;
            request.RequestUri = url;

        }



        private async Task Init()
        {
            {
                if (this._isInit)
                    return;
                var re = new AutoResetEvent(false);
                Debug.WriteLine("Init handler:" + this._isInit);
                var cl = new ServiceMonitorClient(this._discoveryUrl, this._clientId, this._clientSecret);
                Exception error=null;
                var info = await cl.GetServiceAsync(this._id);
                if (info == null || !info.IsRun)
                    throw new ServiceNotFountException(string.Format("Служба '{0}' не запущенна на сервере '{1}'.",
                        this._id, this._discoveryUrl));
                this._serviceUrl = (info.Url??"").TrimEnd('/');
                this._isInit = true;

            }
        }

        //public string ServiceUrl
        //{
        //    get
        //    {
        //        Init();

        //        return this._serviceUrl;
        //    }
        //}

    }

    public class ServiceNotFountException : WikiException
    {
        public ServiceNotFountException(string message) : base(message) { }
    }


}