using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Wiki.DiscoveryService.Common.DTO;
using Wiki.Service.Common;

namespace Wiki.DiscoveryService.Controllers
{
    [RoutePrefix("api/hosts")]
    public class MonitorHostsController : ApiController
    {
        private readonly MonitorMapService _service = new MonitorMapService();
        public static MonitorMap MonitorMap
        {
            get { return MonitorMap.Instance; }
        }

        [Route("")]
        [HttpGet]
        [Authorize]
        public IEnumerable<DTOMonitors> GetHosts()
        {
            try
            {
                var result = MonitorMap.ToDictionary();
                return result.Select(x => new DTOMonitors
                {                  
                    Services = x.Value.Services,
                    HostDetail = new HostDetail()
                    {
                        RemoteIp = x.Value.RemoteIp,
                        MonitorUrl = x.Value.MonitorUrl,
                        IsActive = x.Value.IsActive,
                        LastUpdate = x.Value.LastUpdate,
                        LastBroadcast = x.Value.LastBroadcast,
                        Hostname = x.Value.HostName,
                        IsDebug = x.Value.IsDebug,
                        IsDirect = x.Value.isDirect
                    }                   
                });
            }
            catch (Exception)
            {
                return null;
            }
           
        }
        [Route("groupByServices")]
        [HttpGet]
        [Authorize]
        public IDictionary<string,List<HostDetail>> GetServices()
        {
            try
            {
                return _service.GetServicesWithGroupByHosts();

            }
            catch (Exception)
            {
                return null;
            }

        }

        [Route("networkSettings")]
        [HttpGet]       
        [Authorize]
        public IEnumerable<DTONetworkSettings> GetNetworkSettings()
        {
            var result = MonitorMap.ToDictionary();
            return result.Select(x => new DTONetworkSettings
            {
                IsDebug = x.Value.IsDebug,
                HostName = x.Value.HostName,
                MonitorUrl = x.Value.MonitorUrl,
                IsDirect = x.Value.isDirect,
                RemoteIp = x.Value.RemoteIp
            });
        }

        [Route("{hostName}/add")]
        [HttpPost]
        [Authorize]
        public void AddHost([FromBody] DTONetworkSettings info, string hostName)
        {          
            try
            {
                var monitorUrl = new Uri(info.MonitorUrl).Host;
                MonitorMap.AddOrUpdate(new DiscoveryMonitorNetworkInfo(info.RemoteIp,hostName, info.IsDebug)
                {
                    MonitorUrl = info.MonitorUrl,
                    RemoteIp = monitorUrl.GetIpEndPoint(),
                    isDirect = info.IsDirect
                });
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    e.Message));
            }
        }
        [Route("{hostName}/remove")]
        [HttpPost]
        [Authorize]
        public void DeleteHost([FromBody] DTONetworkSettings info, string hostName)
        {
            try
            {
               var isRemove = MonitorMap.TryDelete(info,hostName);
                if (!isRemove)
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }          
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    e.Message));
            }
        }      
        [Route("directAdd")]
        [AllowAnonymous]
        [HttpPost]
        public void RegisterMonitor([FromBody] ServiceInfoDiscovery info)
        {
            try
            {
                MonitorMap.AddOrUpdate(new DiscoveryMonitorNetworkInfo(null,info.HostName,info.IsDebug)
                {
                    isDirect = true,
                    IsActive = true,
                    RemoteIp = HttpContext.Current.Request.UserHostAddress,
                    LastUpdate = DateTime.Now.ToString("MM/dd/yyyy - HH:mm:ss"),
                    Services = info.Services.ToList()
                    
                });
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    e.Message));
            }
        }
    }
}