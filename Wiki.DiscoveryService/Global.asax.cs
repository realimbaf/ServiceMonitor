using System;
using System.Web.Hosting;
using CarParts.Common.Log;

namespace Wiki.DiscoveryService
{
    public class Global : System.Web.HttpApplication
    {       
        protected void Application_Start(object sender, EventArgs e)
        {
            FileLogger.SetBaseDir(HostingEnvironment.MapPath("~/"));
        }

      
    }
}