using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Wiki.Service.Configuration;

namespace Wiki.ServiceMonitor.Controllers
{
    [RoutePrefix("settings")]
    public class SettingsController : ApiController
    {
        private ServiceConfiguration _serviceConfiguration;

        [Route("{id}")]
        [HttpGet]
        public ServiceConfiguration GetConfiguration(string id)
        {
            _serviceConfiguration = new ServiceConfiguration(id, Environment.CurrentDirectory);

            try
            {
                _serviceConfiguration.Load();
            }
            catch (JsonReaderException e)
            {
                MonitorSrv.Logger.WriteError("err", e);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    e.Message));
            }
            return _serviceConfiguration;
        }

        [Route("{id}")]
        [HttpPost]
        public void SaveConfiguration([FromBody] Dictionary<string, string> configDictionary, string id)
        {
            _serviceConfiguration = new ServiceConfiguration(id, Environment.CurrentDirectory);
            try
            {
                _serviceConfiguration.Load();
                var oldConfig = JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented);
                MonitorSrv.Logger.WriteEvent(String.Format("Service {0} was been changed. The old config: {1}",id, oldConfig));
            }
            catch (Exception e)
            {
                MonitorSrv.Logger.WriteError("err", e);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    e.Message));
            }

           
            _serviceConfiguration = new ServiceConfiguration(id,Environment.CurrentDirectory);
            foreach (var config  in configDictionary)
            {                          
                _serviceConfiguration[config.Key] = config.Value;
            }   
                  
            try
            {
                _serviceConfiguration.Save();
                var newConfig = JsonConvert.SerializeObject(_serviceConfiguration, Formatting.Indented);
                MonitorSrv.Logger.WriteEvent(String.Format("Service {0} was been changed.The new config: {1}",id, newConfig));
            }
            catch (Exception e)
            {

                MonitorSrv.Logger.WriteError("err", e);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    e.Message));
            }                 
        }

    }

}
