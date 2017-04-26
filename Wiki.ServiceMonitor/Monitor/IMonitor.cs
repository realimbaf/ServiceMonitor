using Wiki.Service.Common;
using Wiki.ServiceMonitor.Models;

namespace Wiki.ServiceMonitor.Monitor
{
    public interface IMonitor
    {
        ActiveService GetService(string id);
        void SetActive(string id);
        void SetStopStatus(string id, bool isStop);
        void RemoveActive(string id);

        IServiceManager Manager { get; }

        bool RegisterService(ServiceInfo info);

    }
}