using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wiki.Core.Extensions;
using Wiki.Service.Common;

namespace Wiki.ServiceMonitor.Monitor
{
    public class ServiceCollection
    {
        //private static readonly ServiceCollection _instanse = new ServiceCollection();

        private readonly ReaderWriterLockSlim _syncRoot = new ReaderWriterLockSlim();


        private readonly Dictionary<string, ServiceInfo> _services = new Dictionary<string, ServiceInfo>();

        internal ServiceCollection() { }

        public bool AddOrRefresh(ServiceInfo info)
        {
            info.IsRun = true;

            var service = this[info.Id];
            if (service != null)
            {
                //ожидание запуска нового инстанса и остановка старого
                if(service.Status==ServiceStatus.Starting&&(service.ProcessId==info.ProcessId))
                    return false;
                //сервис ожил
                if (service.Status == ServiceStatus.NotResponse && (service.ProcessId == info.ProcessId))
                {
                    this[info.Id] = info;

                    return true;
                }
                //остановлен вручную
                if (!service.IsRun && (service.ProcessId == info.ProcessId))
                    return false;
                //запущен неуправляемый инстанс
                if (service.IsRun && (service.ProcessId != info.ProcessId))
                    return false;
            }
            else
            {
                
            }
            this[info.Id] = info;


            return true;
        }

        #region Static


        public  ICollection<ServiceInfo> GetAll()
        {
            using (this._syncRoot.UseReadLock())
            {
                return this._services.Values.ToList();
            }
        }

        //public static ServiceInfo GetService(string id)
        //{
        //    return _instanse[id];
        //}

        //public static async Task<bool> Start(string id, string ver=null)
        //{
        //    var service = GetService(id);
        //    if (service != null && service.IsRun)
        //        await Stop(id);
        //    var param = string.Format("-d \"{0}\" -id {1}", MonitorSrv.RepositoryPath, id);
        //    if (!string.IsNullOrEmpty(ver))
        //        param += " -v " + ver;
        //    service=new ServiceInfo
        //    {
        //        Id = id,Version = ver,LastActive = DateTime.Now,Status = ServiceStatus.Starting
        //    };
        //    _instanse[id] = service;
        //    var process = Process.Start("Wiki.ServiceHost.exe", param);
        //    try
        //    {
        //        await WaitForStart(id);
        //        service = GetService(id);
        //        MonitorSrv.Logger.WriteEvent(
        //            "Strart service host. Service id:{0}, version:{1}, start param:{2}, process id:{3}, url:{4}", id,
        //            ver, param, service.ProcessId, service.Url);

        //        return true;

        //    }
        //    catch (OperationCanceledException)
        //    {
        //        MonitorSrv.Logger.WriteWarning(
        //            string.Format(
        //                "Service started and not pulling. Service id:{0}, version:{1}, start param:{2},process id:{3}",
        //                id, ver, param, process != null ? process.Id + "" : "process is null"));
        //        throw;
        //        return false;

        //    }
        //    catch (Exception e)
        //    {
        //        MonitorSrv.Logger.WriteError(string.Format("Service not started. Service id:{0}, version:{1}, start param:{2}"), e);
        //        throw ;
        //    }
        //}

        //private static async Task WaitForStart(string id, int timeout = 5000)
        //{
        //    var tokenSource = new CancellationTokenSource(timeout);
        //    var token = tokenSource.Token;
        //    var task = Task.Factory.StartNew(() =>
        //    {
        //        while (true)
        //        {
        //            if (token.IsCancellationRequested)
        //                token.ThrowIfCancellationRequested();
        //            var servise = GetService(id);
        //            if (servise != null && servise.IsRun)
        //                break;
        //            Thread.Sleep(100);
        //        }

        //    }, token);
        //    await task;
        //}

        //public static async Task Stop(string id,bool isManual=false)
        //{
        //    var service = ServiceCollection.GetService(id);
        //    if (service == null)
        //        throw new KeyNotFoundException(string.Format("Service '{0}' not found", id));
        //    if (!service.IsRun)
        //        throw new InstanceNotFoundException(string.Format("Service '{0}' is stoped", id));
        //    service.IsRun = false;
        //    service.Status=ServiceStatus.Stoping;
        //    MonitorSrv.Logger.WriteEvent("Sent to service host stop command. Service id:{0}, process id:{0}");

        //    var cl = new HttpClient();
        //    var result = await cl.GetAsync(service.Url + "config/stop");
        //    result.EnsureSuccessStatusCode();
        //}

        #endregion



        public ServiceInfo this[string id]
        {
            get
            {
                using (this._syncRoot.UseReadLock())
                {
                    if (this._services.ContainsKey(id))
                        return this._services[id];
                    return null;

                }
            }
            set
            {
                this._syncRoot.EnterWriteLock();
                try
                {
                    this._services[id] = value;
                }
                finally
                {
                    this._syncRoot.ExitWriteLock();
                }

            }
        }

    }
}