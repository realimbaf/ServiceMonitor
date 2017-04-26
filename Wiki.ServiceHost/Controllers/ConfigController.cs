using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using CarParts.Common.Log;
using Wiki.Service.Configuration;
using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Controllers
{
    [RoutePrefix("config")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ConfigController : ApiController
    {

        [Route("~/status")]
        [HttpGet]
        public HttpResponseMessage InfoPage()
        {                                                                         
            var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Wiki.ServiceHost.Content.Pages.Status.html");
            var infos = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            if(stream==null)
                throw new KeyNotFoundException(string.Join("\n",infos));
            using (StreamReader reader = new StreamReader(stream))
            {
                var page= reader.ReadToEnd();
                return new HttpResponseMessage() { Content = new StringContent(page, System.Text.Encoding.UTF8, "text/html") };
            }

            
        }

        public class LogInfo
        {
            public int AddedLogCount { get; set; }
            public int SavedLogCount { get; set; }
            public int ErrorCount { get; set; }
            public string LastError { get; set; }
            public string Name { get; set; }
        }


        [Route("~/loginfo")]
        [HttpGet]
        [AllowAnonymous]
        public LogInfo GetLofInfo()
        {
            var res= FileLogger.Statictic;
            return new LogInfo
            {
                AddedLogCount = res.AddedLogCount,
                ErrorCount = res.ErrorCount,
                LastError = res.LastError,
                SavedLogCount = res.SavedLogCount,
                Name = res.GetType().Assembly.FullName
            };

        }

        [Route("")]
        [HttpGet]
        public KeyValuePair<string, string>[] GetConfigs()
        {
            
            return ConfigurationContainer.Configuration.ToArray();
        }


        [Route("reload")]
        [HttpGet]
        public KeyValuePair<string, string>[] ReloadConfigs()
        {
            ConfigurationContainer.Configuration.Load();
            return ConfigurationContainer.Configuration.ToArray();
        }

        [Route("")]
        [HttpPost]
        public KeyValuePair<string, string>[] SaveConfig(SaveConfigRequest request)
        {
            if (request != null)
            {
                ConfigurationContainer.Configuration[request.Key] = request.Value;
                ConfigurationContainer.Configuration.Save();

            }
            return ConfigurationContainer.Configuration.ToArray();
        }



        [Route("stop")]
        [HttpGet]
        public async Task<string> Stop()
        {
            MainSrv.StopAsync();
            return "Ok";
        }

        [Route("status")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<StatusInfo> Status()
        {
            var proc = Process.GetCurrentProcess();
            var result = new StatusInfo
            {
                BasePriority=proc.BasePriority,
                ServiceId=MainSrv.PackageInfo.Id,
                ProccessId=proc.Id,
                MachineName = proc.MachineName,
                PagedMemorySize64 = proc.PagedMemorySize64,
                PagedSystemMemorySize64 = proc.PagedSystemMemorySize64,
                PeakPagedMemorySize64 = proc.PeakPagedMemorySize64,
                PeakVirtualMemorySize64 = proc.PeakVirtualMemorySize64,
                PeakWorkingSet64 = proc.PeakWorkingSet64,
                PrivateMemorySize64 = proc.PrivateMemorySize64,
                ProcessName = proc.ProcessName,
                PrivilegedProcessorTime = proc.PrivilegedProcessorTime,
                SessionId = proc.SessionId,
                StartTime = proc.StartTime,
                TotalProcessorTime = proc.TotalProcessorTime,
                UserProcessorTime = proc.UserProcessorTime,
                VirtualMemorySize64 = proc.VirtualMemorySize64,
                WorkingSet64 = proc.WorkingSet64,
                Assemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.ToString()).ToArray(),
                NugetFile = MainSrv.PackageInfo.NugetFile,
            };
            return result;
        }

        public class SaveConfigRequest
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        //[Route("test")]
        //[HttpGet]
        //public HttpResponseMessage Test()
        //{
        //    var cfg = this.Configuration.Services.GetApiExplorer().ApiDescriptions.ToLookup(api => api.ActionDescriptor.ControllerDescriptor)
        //        .Select(x=>new
        //        {
        //            x.Key.ControllerName,
        //            Api=x.Select(c=>new
        //            {
        //                c.HttpMethod.Method,
        //                c.RelativePath,
        //                c.Documentation,
        //                Parametrs=c.ParameterDescriptions.Select(p=>new{p.ParameterDescriptor.ParameterName,p.ParameterDescriptor.ParameterType.Name})
                        
        //            })
        //        })
        //        ;
        //    return new HttpResponseMessage() { Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(cfg), System.Text.Encoding.UTF8, "text/html") }; ;
        //}
    }
}