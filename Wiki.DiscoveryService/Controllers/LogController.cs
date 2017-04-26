using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;

namespace Wiki.DiscoveryService.Controllers
{
    [Authorize]
    [RoutePrefix("api/logs")]
    public class LogController : ApiController
    {
        private static string _dir = HostingEnvironment.MapPath("~/Log");

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

        [Route("")]
        [HttpGet]
        public string[] GetLogFiles()
        {
            var dir = new DirectoryInfo(LogDir);
            var files = dir.GetFiles("*.log");
            return files.Select(x => x.Name).Reverse().ToArray();
        }

        [Route("{file}")]
        [HttpGet]
        public HttpResponseMessage GetLogFile(string file)
        {
            var fn = Path.Combine(LogDir, file);
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
