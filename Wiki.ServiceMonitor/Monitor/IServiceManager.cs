using System.Collections.Generic;
using Wiki.Service.Common;

namespace Wiki.ServiceMonitor.Monitor
{
    public interface IServiceManager
    {
        ServiceInfo GetService(string id);
        ICollection<ServiceInfo> GetServices();
        bool RegisterService(ServiceInfo info);
    }
}