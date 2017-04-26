using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace Wiki.ServiceMonitor.Controllers
{
    /// <summary>
    /// Получение лог файлов
    /// </summary>
    [RoutePrefix("log")]
    public class LogController : ApiController
    {
        private static string _dir;

        internal string LogDir
        {
            get
            {
                if (_dir == null)
                {
                    _dir = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                    _dir = Path.Combine(_dir, "Log");
                }
                return _dir;
            }
        }

        /// <summary>
        /// Фозвращает список всех логов
        /// </summary>
        /// <returns></returns>
        [Route("all")]
        [HttpGet]
        public string[] GetLogFiles()
        {
            var dir = new DirectoryInfo(this.LogDir);
            var files = dir.GetFiles("*.log");
            return files.Select(x => x.Name).ToArray();
        }
        /// <summary>
        /// Фозвращает список всех логов
        /// </summary>
        /// <returns></returns>
        [Route("all/{id}")]
        [HttpGet]
        public IEnumerable<string> GetLogByService(string id)
        {
            var dir = new DirectoryInfo(this.LogDir);
            var files = dir.GetFiles("*.log");
            return files.Where(x => x.Name.Contains(id))
                .Select(x => x.Name)
                .Select(x => DateTime.Parse(Regex.Match(x, @"\d{4}\.\d{2}\.\d{2}").Value.Replace(".", "-")))
                .OrderByDescending(x => x)
                .Select(x => String.Format("{0}_{1:yyyy.MM.dd}.log",id, x));
        }
        /// <summary>
        /// Получить окнкретнйы лог
        /// </summary>
        /// <example>dsfsdfsdfasd</example>
        /// <param name="file"></param>
        /// <returns></returns>
        [Route("file/{file}")]
        [HttpGet]
        public HttpResponseMessage GetLogFile(string file)
        {
            var fn = Path.Combine(this.LogDir, file);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(File.ReadAllBytes(fn))
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = file
            };
            return result;
        }

    }
}