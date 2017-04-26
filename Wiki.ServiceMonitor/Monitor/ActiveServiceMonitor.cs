using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Newtonsoft.Json;
using Wiki.Core.Extensions;
using Wiki.Service.Common;
using Wiki.Service.Configuration;
using Wiki.ServiceMonitor.Models;

namespace Wiki.ServiceMonitor.Monitor
{
    internal class ActiveServiceMonitor : IMonitor
    {
        const string ConfigKey = "ActiveServises";


        private Timer _timer;
        private readonly Dictionary<string, ActiveService> _activeServices = new Dictionary<string, ActiveService>();
        private readonly int _interval = 1000;
        private readonly FileLogger _logger;
        private readonly ServiceManager _serviceManager;
        private readonly ReaderWriterLockSlim _syncRoot = new ReaderWriterLockSlim();

        public ActiveServiceMonitor(FileLogger logger, ServiceManager serviceManager)
        {
            this._logger = logger;
            this._serviceManager = serviceManager;
            LoadConfig();

        }

        public IServiceManager Manager
        {
            get { return this._serviceManager; }
        }

        public void SetActive(string id)
        {
            var service = GetOrCreateEmpty(id);
            service.IsManualStop = false;

            SaveConfig();
        }

        public void RemoveActive(string id)
        {
            var service = this[id];
            if (service != null)
                using (this._syncRoot.UseWriteLock())
                {
                    this._activeServices.Remove(id);
                }
            SaveConfig();
        }



        public void SetStopStatus(string id, bool isStop)
        {
            var service = this[id];
            if (service != null)
                service.IsManualStop = isStop;

        }



        private void SaveConfig()
        {
            var cfg = ConfigToString();
            ConfigurationContainer.Configuration[ConfigKey] = cfg;
            ConfigurationContainer.Configuration.Save();
            this._logger.WriteEvent("Save active service configuration:{0}{1}", Environment.NewLine, cfg);
        }

        private string ConfigToString()
        {
            string cfg = JsonConvert.SerializeObject(GetAll());
            return cfg;
        }

        public List<ActiveService> GetSavedConfig()
        {
            try
            {
                var cfg = ConfigurationContainer.Configuration[ConfigKey];
                if (!string.IsNullOrWhiteSpace(cfg))
                {
                    var config = JsonConvert.DeserializeObject<List<ActiveService>>(cfg);
                    return config;
                }

            }
            catch(Exception e) { }
            return new List<ActiveService>();

        }

        private void LoadConfig()
        {
            var config = GetSavedConfig();

            if (config!=null)
            {
                using (this._syncRoot.UseWriteLock())
                {
                    this._activeServices.Clear();
                    config.ForEach(x =>
                    {
                        this._activeServices[x.Id] = x;
                        x.IsManualStop = false;
                    });

                }
            }
            this._logger.WriteEvent("Loaded active service configuration:{0}{1}", Environment.NewLine, ConfigToString());

        }

        private ActiveService GetOrCreateEmpty(string id)
        {
            var service = this[id];
            if (service == null)
            {
                service = new ActiveService { Id = id, IsManualStop = true };
                this[id] = service;
            }
            return service;
        }

        public ActiveService GetService(string id)
        {
            return this[id];
        }


        public ICollection<ActiveService> GetAll()
        {
            using (this._syncRoot.UseReadLock())
            {
                return this._activeServices.Values.ToList();
            }
        }

        public ActiveService this[string id]
        {
            get
            {
                using (this._syncRoot.UseReadLock())
                {
                    if (this._activeServices.ContainsKey(id))
                        return this._activeServices[id];
                    return null;
                }
            }
            set
            {
                using (this._syncRoot.UseWriteLock())
                {
                    this._activeServices[id] = value;
                }
            }
        }

        public async Task<bool> StartService(string id, string ver)
        {
            SetStopStatus(id, false);

            return await this._serviceManager.Start(id, ver);
        }

        public async Task StopService(string id)
        {
            await this._serviceManager.Stop(id);
            SetStopStatus(id, true);
        }

        public bool RegisterService(ServiceInfo info)
        {
            if (info == null)
            {
                return false;
            }

            return this._serviceManager.RegisterService(info);
        }

        public void Start()
        {
            this._logger.WriteEvent("Monitoring start");

            TurnOnTimer();
        }

        public void Stop()
        {
            this._logger.WriteEvent("Monitoring stop");
            if (this._timer != null)
            {
                this._timer.Change(-1, -1);
                this._timer = null;
            }
        }

        private void TurnOnTimer()
        {
            if (this._timer == null)
                this._timer = new Timer(TimerProcess, null, this._interval * 5, Timeout.Infinite);
            else
                this._timer.Change(this._interval, Timeout.Infinite);
        }

        private void TimerProcess(object state)
        {
            try
            {
                CheckProcess();
            }
            catch (Exception e)
            {
                this._logger.WriteError("Monitoring error.", e);
            }

            TurnOnTimer();

        }

        private void CheckProcess()
        {
            CheckRunning();

            var services = GetAll();
            foreach (var activeService in services)
            {
                if (activeService.IsManualStop)
                    continue;
                var service = this.Manager.GetService(activeService.Id);
                if (activeService.Version == null && service != null)
                {
                    activeService.Version = service.Version;
                    SaveConfig();
                }
                if (service == null || !service.IsRun)
                {
                    if (service != null && service.Status == ServiceStatus.Starting)
                        continue;
                    this._logger.WriteEvent("Scheduler start active service.Id:{0} version:{1}", activeService.Id, activeService.Version);
                    this._serviceManager.Start(activeService.Id, activeService.Version).Wait();
                }

            }
        }

        private void CheckRunning()
        {
            var services = this.Manager.GetServices();
            foreach (var service in services)
            {
                var time = (DateTime.Now - service.LastActive).TotalSeconds;
                if (time > 10)
                {
                    service.IsRun = false;
                    service.Status = ServiceStatus.NotResponse;
                }

            }
        }
    }
}