using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wiki.ApiClient.Core;
using Wiki.Core.Exceptions;
using Wiki.Core.Helpers;
using Wiki.Service.Common;

namespace Wiki.Service.Common.Clients
{
    public class ServiceMonitorClient : BaseClient
    {
        private string _clientId;
        private string _clientSecret;
        private static AccessTokenResponse _token;

        private static string _identityServerUrl ;

        internal ServiceMonitorClient()
            : base("http://127.0.0.1:9100/")
        {
            var cred = DiscoveryFactory.GetDefaultCreditenals();
            if (cred != null)
            {
                this._clientId = cred.ClientId;
                this._clientSecret = cred.ClientSecret;
            }
        }

        public ServiceMonitorClient(string baseUrl, string clientId, string clientSecret)
            : base(baseUrl)
        {
            this._clientId = clientId;
            this._clientSecret = clientSecret;
        }



        public string IdentityServerUrl 
        {
            get
            {
                if (_identityServerUrl == null)
                {
                    _identityServerUrl = this.GetIdentityServerUrl();
                }
                return _identityServerUrl;
            }
        }

        private static DateTime _oldTokenTime=DateTime.MinValue;

        [Obsolete("Use GetClientAsync")]
        protected override  HttpClient GetClient()
        {
            var cl = AsyncHelpers.RunSync(GetClientAsync);
            return cl;
        }


        protected override async Task<HttpClient> GetClientAsync()
        {
            var cl = await base.GetClientAsync();

            var reload = this.IsTimeReload;

            var token = await GetTokenAsync(reload);
            cl.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.TokenType, token.Token);

            return cl;

        }

        //internal AccessTokenResponse GetToken(bool reload=false)
        //{
        //    var token = AsyncHelpers.RunSync(() => GetTokenAsync(reload));
        //    return token;
        //}

        internal async Task<AccessTokenResponse> GetTokenAsync(bool reload = false)
        {

            if (_token == null || reload || this.IsTimeReload)
            {
                Debug.WriteLine("GetTokenAsync reload:{0} isTime:{2} token:{1} ",reload,_token, this.IsTimeReload);
                AccessTokenResponse token = null;
                token = await GetNewToken();
                _token = token;
                _oldTokenTime = DateTime.Now;
            }

            return _token;

        }

        private bool IsTimeReload
        {
            get { return (DateTime.Now - _oldTokenTime).TotalSeconds > 20; }
        }

        public async Task<AccessTokenResponse> GetNewToken()
        {
            var clientId = this._clientId;
            var clientSecret = this._clientSecret;
            var identityServerUrl = await GetIdentityServerUrlAsync();
            

            var token= await DiscoveryFactory.GetToken(clientId, clientSecret, identityServerUrl).ConfigureAwait(true);
            if (token.IsError)
            {
                throw new AuthenticationException("Error get token. " + token.Error);
            }
            return token;
        }

        internal async Task<bool> RegisterAsync(ServiceInfo info)
        {
            using (var cl = await GetClientAsync())
            {
                var str = JsonConvert.SerializeObject(info);
                var content=new StringContent(str,Encoding.UTF8,"application/json");
                var result = await cl.PostAsync("services/register", content);
                if (!result.IsSuccessStatusCode)
                {
                    var message = await result.Content.ReadAsStringAsync();
                    throw new WikiApiException(result.StatusCode, message);
                }
                return JsonConvert.DeserializeObject<bool>(await result.Content.ReadAsStringAsync());

            }
        }

        internal bool Register(ServiceInfo info)
        {
            return AsyncHelpers.RunSync(() => RegisterAsync(info));
        }

        public async Task<ICollection<ServiceInfo>> GetServicesAsync()
        {
            var cl =await  GetClientAsync();
            var result = await cl.GetAsync("services");
            if (!result.IsSuccessStatusCode)
            {
                var message = await result.Content.ReadAsStringAsync();
                throw new WikiApiException(result.StatusCode, message);
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ICollection<ServiceInfo>>(await result.Content.ReadAsStringAsync());
        }
        public async Task<ServiceInfoDiscovery> GetMonitorServicesAsync()
        {
            var cl = await GetClientAsync();
            var result = await cl.GetAsync("services/monitorstatus");
            if (!result.IsSuccessStatusCode)
            {
                var message = await result.Content.ReadAsStringAsync();
                throw new WikiApiException(result.StatusCode, message);
            }
            return JsonConvert.DeserializeObject<ServiceInfoDiscovery>(await result.Content.ReadAsStringAsync());
        }

        public async Task<ServiceInfo> GetServiceAsync(string id)
        {
            var cl = await GetClientAsync();
            var result = await cl.GetAsync("services/status/"+id);
            if (!result.IsSuccessStatusCode)
            {
                var message = await result.Content.ReadAsStringAsync();
                throw new WikiApiException(result.StatusCode, message);

            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceInfo>(await result.Content.ReadAsStringAsync());
        }

        //public ServiceInfo GetService(string id)
        //{
        //    return AsyncHelpers.RunSync(()=>GetServiceAsync(id));
        //}

        internal async Task<string> GetIdentityServerUrlAsync()
        {
            var cl = await base.GetClientAsync();
            var result = await cl.GetAsync("identityserver");
            if (!result.IsSuccessStatusCode)
            {
                var message = await result.Content.ReadAsStringAsync();
                throw new WikiApiException(result.StatusCode, message);

            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<string>(await result.Content.ReadAsStringAsync());

        }

        internal string GetIdentityServerUrl()
        {
            return  AsyncHelpers.RunSync(GetIdentityServerUrlAsync);
        }

    }
}
