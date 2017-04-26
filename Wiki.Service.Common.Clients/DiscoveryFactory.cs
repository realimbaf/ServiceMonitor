using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Wiki.Service.Common.Clients
{
    /// <summary>
    /// Фабрика для DiscoveryService
    /// </summary>
    public class DiscoveryFactory
    {
        private static ClientCreditenals _cred;

        /// <summary>
        /// Устанавливает параметры аутентификации по умолчанию
        /// </summary>
        /// <param name="cred"></param>
        public static void SetDefaultCreditenals(ClientCreditenals cred)
        {
            _cred = cred;
        }

        /// <summary>
        /// Получает параметры аутентификации по умолчанию
        /// </summary>
        /// <returns></returns>
        public static ClientCreditenals GetDefaultCreditenals()
        {
            if (_cred == null)
                return null;
            return new ClientCreditenals
            {
                ClientId = _cred.ClientId,
                ClientSecret = _cred.ClientSecret
            };
        }

        /// <summary>
        /// Получает новый токен
        /// </summary>
        /// <param name="identityServerUrl"></param>
        /// <returns></returns>
        public static async Task<AccessTokenResponse> GetToken(string identityServerUrl)
        {

            return await GetToken(_cred.ClientId, _cred.ClientSecret, identityServerUrl);
        }

        /// <summary>
        /// Получает новый токен
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="identityServerUrl"></param>
        /// <returns></returns>
        public static async Task<AccessTokenResponse> GetToken(string clientId, string clientSecret, string identityServerUrl)
        {
            var sw = Stopwatch.StartNew();
            var headerValue =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", clientId, clientSecret)));
            var cl = new HttpClient();

            cl.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);
            var content =
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"scope", "service"}
                });

            var result = await cl.PostAsync(identityServerUrl + "/connect/token", content);
            //Debug.WriteLine("GetToken init:{0}", sw.ElapsedMilliseconds);

            var tokenResult = new AccessTokenResponse();
            if (!result.IsSuccessStatusCode)
            {
                tokenResult.IsError = true;
                tokenResult.Error = result.ReasonPhrase;
            }
            else
            {
                var json = await result.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);
                tokenResult.Token = obj.GetValue("access_token").ToString();
                tokenResult.TokenType = obj.GetValue("token_type").ToString();
                tokenResult.Expires = obj.GetValue("expires_in").Value<int>();
            }
            //Debug.WriteLine("GetToken:{0}",sw.ElapsedMilliseconds);
            return tokenResult;
        }
    }

    /// <summary>
    /// Параметры аутентификации слиента
    /// </summary>
    public class ClientCreditenals
    {
        /// <summary>
        /// Id клиента (логин)
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Секретный ключ (пароль)
        /// </summary>
        public string ClientSecret { get; set; }
    }
                 
}