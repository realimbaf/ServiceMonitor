using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Wiki.DiscoveryService.Common;
using Wiki.Service.Common;

namespace Wiki.DiscoveryService.Controllers
{
    [RoutePrefix("services")]
    public class MonitorServicesController : ApiController
    {
        private readonly MonitorMapService _service = new MonitorMapService();
        public static MonitorMap MonitorMap
        {
            get { return MonitorMap.Instance; }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("~/identityserver")]
        public string GetIdentityServerUrl()
        {
            return Constants.IdentityServerUrl;
        }


        [Route("status/{id}")]
        [HttpGet]
        [Authorize]
        public ServiceInfoBase GetServiceById(string id)
        {
            try
            {
                return _service.GetFirstService(id);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    e.Message));
            }

        }
    }
}
