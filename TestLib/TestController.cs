using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CarParts.Common;
using Owin;
using Wiki.Service.Configuration;

namespace TestLib
{
    [RoutePrefix("api1/qqq1")]
    public class TestController : ApiController
    {

       
        public async Task<string> Test()
        {
            return "Api worck";
        }

    }

    public class test:IServiceConfig
    {
        public void Init(IAppBuilder app, HttpConfiguration config)
        {
            
        }

        public void Start()
        {
            Arhivator.SetTempDir("qqq");
        }

        public void Stop()
        {
        }
    }
}
