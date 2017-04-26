using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Owin;

namespace Wiki.Service.Configuration
{
    /// <summary>
    /// Предоставляет настройку службы
    /// </summary>
    public interface IServiceConfig
    {

        /// <summary>
        /// Вызывается при конфигурировании web api
        /// </summary>
        /// <param name="app"></param>
        /// <param name="config"></param>
        void Init(IAppBuilder app, HttpConfiguration config);

        /// <summary>
        /// Запускает фоновые процессы
        /// </summary>
        void Start();

        /// <summary>
        /// Останавливает фоновые процессы
        /// </summary>
        void Stop();
    }
}
