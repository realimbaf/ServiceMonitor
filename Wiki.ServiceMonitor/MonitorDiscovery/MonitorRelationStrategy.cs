using System;
using System.Threading;
using Wiki.Core.Scheduler;

namespace Wiki.ServiceMonitor.MonitorDiscovery
{
    public abstract class MonitorRelationStrategy
    {
        private Timer _tickTimer { get; set; }
        protected const int PORT = 31000;
        protected int _localport;
        protected MonitorRelationStrategy(int? localport = null)
        {
            _localport = localport ?? PORT;
        }

        public void Start()
        {
            if (_tickTimer == null)
            {
                _tickTimer = new Timer(SendTick);
            }
            _tickTimer.Change(0, 10000);
        }

        public void Stop()
        {
            if (_tickTimer != null)
            {
                _tickTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _tickTimer = null;
            }
        }  
        protected abstract void SendTick(object obj);
    }


}
