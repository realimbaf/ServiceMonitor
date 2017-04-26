using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Wiki.Service.Common;
using Wiki.ServiceMonitor.Models;
using Wiki.ServiceMonitor.Utils;

namespace Wiki.ServiceMonitor.Controllers
{
    [RoutePrefix("services")]
    public class ServiceController : ApiController
    {

        [HttpGet]
        [AllowAnonymous]
        [Route("~/identityserver")]
        public string GetIdentityServerUrl()
        {
            return Startup.GetIdentityServerUrl();
        }

        [Route("")]
        [HttpGet]
        public ICollection<ServiceInfoExt> GetServices()
        {
            var serviceInfos = MonitorSrv.Monitor.Manager.GetServices();
            var result = new List<ServiceInfoExt>(serviceInfos.Count);
            var savedConfigs = MonitorSrv.Monitor.GetSavedConfig();
            foreach (var info in serviceInfos)
            {
                var item = CreateServiceInfo(info,savedConfigs);
                result.Add(item);

            }
            return result;
        }

        [Route("monitorstatus")]
        [HttpGet]
        public ServiceInfoDiscovery GetMonitorsStatus()
        {
            var serviceInfos = MonitorSrv.Monitor.Manager.GetServices();
            var servicesList = new List<ServiceInfo>(serviceInfos.Count);
            var savedConfigs = MonitorSrv.Monitor.GetSavedConfig();
            foreach (var info in serviceInfos)
            {
                var item = CreateServiceInfo(info, savedConfigs);
                servicesList.Add(item);
            }
            return new ServiceInfoDiscovery(MonitorSettings.IsDebug)
            {
                Services = servicesList,                      
            };
        }

        [Route("status/{id}")]
        [HttpGet]
        public ServiceInfoExt GetServiceById(string id)
        {
            var serviceInfo = MonitorSrv.Monitor.Manager.GetService(id);
            if (serviceInfo == null)
            {
                return null;
            }
            var item = CreateServiceInfo(serviceInfo);
            
            return item;

        }


        [Route("list")]
        [HttpGet]
        public IEnumerable<string> ListPackages()
        {
            var names = new DirectoryInfo(MonitorSrv.RepositoryPath).GetFiles("*.nupkg").Select(x => x.Name).ToArray();
            var result =
                names.Select(x => Regex.Match(x, "^(?<id>.+?)(\\.\\d+){2,4}\\.nupkg").Groups["id"])
                    .Where(x => x.Success)
                    .Select(x => x.Value)
                    .Distinct()
                    .OrderBy(x=>x)
                    .ToList();
            return result;

        }

        [Route("list/{id}")]
        [HttpGet]
        public IEnumerable<string> ListPackages(string id)
        {
            var mask = string.Format("{0}.*.nupkg",id);
            var names = new DirectoryInfo(MonitorSrv.RepositoryPath).GetFiles(mask).Select(x => x.Name).ToArray();
            var result =names.Select(x=>x.Replace(id+".","").Replace(".nupkg",""))
                    .Distinct()
                    .ToList();

            result.Sort(new VersionCompare());
            return result;

        }



        [Route("start/{id}")]
        [HttpGet]
        public async Task<string> Start(string id,string ver)
        {
            bool result;
            try
            {
                result = await MonitorSrv.Monitor.StartService(id, ver);

            }
            catch (Exception e)
            {
                MonitorSrv.Logger.WriteError("err",e);
                throw new AggregateException(e);
            }
            if (!result)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Service not started: timeout start"));
            return "ok";
        }

        [Route("stop/{id}")]
        [HttpGet]
        public async Task<string> Stop(string id)
        {
            MonitorSrv.Monitor.SetStopStatus(id,true);
            try
            {
                await MonitorSrv.Monitor.StopService(id);
            }
            catch (InstanceNotFoundException e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
            catch (KeyNotFoundException e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
            catch (HttpRequestException e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadGateway, e.InnerException.Message));

            }
            return "ok";
        }

        [Route("setactive/{id}")]
        [HttpGet]
        public async Task<bool> SetActive(string id)
        {
            MonitorSrv.Monitor.SetActive(id);
            return true;
        }

        [Route("removeactive/{id}")]
        [HttpGet]
        public async Task<bool> RemoveActive(string id)
        {
            MonitorSrv.Monitor.RemoveActive(id);
            return true;
        }

        [Route("register")]
        [HttpPost]
        public bool RegisterService(ServiceInfo info)
        {
            if (info == null)
            {
                return true;
            }
            return MonitorSrv.Monitor.RegisterService(info);
        }

        private static ServiceInfoExt CreateServiceInfo(ServiceInfo info, List<ActiveService> savedConfigs=null)
        {
            if (savedConfigs == null)
                savedConfigs = MonitorSrv.Monitor.GetSavedConfig();
            var item = new ServiceInfoExt
            {
                Id = info.Id,
                Url = info.Url,
                Version = info.Version,
                IsRun = info.IsRun,
                ProcessId = info.ProcessId,
                LastActive = info.LastActive,
                Status = info.Status,

            };

            var service = MonitorSrv.Monitor.GetService(item.Id);
            item.IsAuto = service != null;
            if (item.IsAuto)
                item.IsManualStop = service.IsManualStop;
            var savedConfig = savedConfigs.FirstOrDefault(x => x.Id == item.Id);
            if (savedConfig != null)
                item.ActiveVersion = savedConfig.Version;

            return item;
        }
    }


    
}