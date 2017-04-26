using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Wiki.ApiClient.Core;
using Wiki.Core.Exceptions;

namespace Wiki.Service.Common.Clients
{
    /// <summary>
    /// Базовый клиент сервиса
    /// </summary>
    public abstract class ServiceClientBase : BaseClient,IDisposable
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        protected ServiceHttpMessageHandler _handler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discoveryAdress"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        protected ServiceClientBase(string discoveryAdress, string clientId, string clientSecret)
            : base(discoveryAdress)
        {
            Debug.WriteLine("begin init");
            this._clientId = clientId;
            this._clientSecret = clientSecret;
            Init();
        }

        private void Init()
        {
            this._handler = new ServiceHttpMessageHandler(this.ServiceId, this.BaseUrl.AbsoluteUri,this._clientId,this._clientSecret);
        }


        /// <summary>
        /// Id сервиса, к которому обращается клиент
        /// </summary>
        public abstract string ServiceId { get; }


        /// <summary>
        /// Возвращает подготовленный <see cref="HttpClient"/> для отправки запроса
        /// </summary>
        /// <returns></returns>
        protected override HttpClient GetClient()
        {
            var client = new ServiceHttpClient(this._handler);
            return client;
        }

        protected async override Task<HttpClient> GetClientAsync()
        {
            return GetClient();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this._handler != null)
            {
                this._handler.Dispose();
                this._handler = null;
            }
        }


        /// <summary>
        /// Начало пути (префикс) URL контроллера
        /// </summary>
        public virtual string FullUrl { get { return ""; } }

        /// <summary>
        /// Асинхронно отправляет данные методом POST по адресу <see cref="this.FullUrl"/>+<see cref="command"/>
        /// </summary>
        /// <typeparam name="T">Тип, к которому будет приведен результат</typeparam>
        /// <param name="command">Адресс запроса</param>
        /// <param name="model">Данные, которые будут отправленны в запросе</param>
        /// <returns>Ответ сервиса приведенный к типу <see cref="T"/></returns>
        ///<exception cref="WikiApiException"></exception>
        protected async Task<T> PostData<T>(string command, object model)
        {
            var result = await this.HttpPostAsync(command, model);
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Асинхронно отправляет данные методом POST по адресу <see cref="this.FullUrl"/>+<see cref="command"/> без получения результата.
        /// </summary>
        /// <param name="command">Адресс запроса</param>
        /// <param name="model">Данные, которые будут отправленны в запросе</param>
        /// <returns></returns>
        ///<exception cref="WikiApiException"></exception>
        protected async Task PostData(string command, object model)
        {
            var result = await this.HttpPostAsync(command, model);
        }

        /// <summary>
        /// Асинхронно отправляет данные методом POST по адресу <see cref="this.FullUrl"/>+<see cref="command"/>
        /// </summary>
        /// <param name="command">Адресс запроса</param>
        /// <param name="model">Данные, которые будут отправленны в запросе</param>
        /// <returns></returns>
        ///<exception cref="WikiApiException"></exception>
        protected async Task<HttpResponseMessage> HttpPostAsync(string command, object model)
        {
            HttpContent content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8,
                "application/json");
            var client = await GetClientAsync();
            var result = await client.PostAsync(FullUrl + command, content);

            if (!result.IsSuccessStatusCode)
            {
                var msg = await result.Content.ReadAsStringAsync();
                throw new WikiApiException(result.StatusCode, msg);
            }
            return result;
        }


        /// <summary>
        /// Асинхронно отправляет данные методом GET по адресу <see cref="this.FullUrl"/>+<see cref="command"/>
        /// </summary>
        /// <typeparam name="T">Тип, к которому будет приведен результат</typeparam>
        /// <param name="model">Данные, которые будут отправленны в запросе</param>
        /// <param name="command">Адресс запроса</param>
        /// <returns>Ответ сервиса приведенный к типу <see cref="T"/></returns>
        ///<exception cref="WikiApiException"></exception>
        protected Task<T> GetData<T>(string command, object model)
        {
            command += "?" + model.GetUrlParams();
            return GetData<T>(command);
        }

        /// <summary>
        /// Асинхронно отправляет данные методом GET по адресу <see cref="this.FullUrl"/>+<see cref="command"/>
        /// </summary>
        /// <typeparam name="T">Тип, к которому будет приведен результат</typeparam>
        /// <param name="command">Адресс запроса</param>
        /// <returns>Ответ сервиса приведенный к типу <see cref="T"/></returns>
        ///<exception cref="WikiApiException"></exception>
        protected async Task<T> GetData<T>(string command)
        {
            var result = await this.HttpGetAsync(command);
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Асинхронно отправляет данные методом GET по адресу <see cref="this.FullUrl"/>+<see cref="command"/>
        /// </summary>
        /// <param name="command">Адресс запроса</param>
        /// <returns></returns>
        ///<exception cref="WikiApiException"></exception>
        protected async Task<HttpResponseMessage> HttpGetAsync(string command)
        {
            var client = await GetClientAsync();
            var result = await client.GetAsync(FullUrl + command);
            if (!result.IsSuccessStatusCode)
            {
                var msg = await result.Content.ReadAsStringAsync();
                throw new WikiApiException(result.StatusCode, msg);
            }
            return result;
        }

    }
}