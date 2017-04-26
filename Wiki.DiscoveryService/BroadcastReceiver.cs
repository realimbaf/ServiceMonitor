using System;
using System.Threading;
using CarParts.Common.Log;
using Wiki.DiscoveryService.Common;
using Wiki.Service.Common;

namespace Wiki.DiscoveryService
{
    public class BroadcastReceiver
    {
        private const int PORT = 31000;
        private const int INTERVAL = 15000;
        private Timer _timer;
        private readonly ListeningService _listeningService;
        private readonly FileLogger _loger;



        public static MonitorMap MonitorMap
        {
            get { return MonitorMap.Instance; }
            
        }
        public BroadcastReceiver()
        {
            _listeningService = new ListeningService(PORT);
            _loger = new FileLogger("discovery");
        }

        public void Start()
        {
            _loger.WriteEvent("BroadcastReceiver start");
            TurnOnTimer();
        }

        public void Stop()
        {
            _loger.WriteEvent("BroadcastReceiver stop");
            if (this._timer != null)
            {
                this._timer.Change(-1, -1);
                this._timer = null;
            }
        }

        private void TurnOnTimer()
        {
            if (this._timer == null)
                this._timer = new Timer(ListeningTick, null, INTERVAL, Timeout.Infinite);
            else
                this._timer.Change(INTERVAL, Timeout.Infinite);
        }

        private async void ListeningTick(object state)
        {
            try
            {
                var networkInfo = await _listeningService.StartListeningAsync() as DiscoveryMonitorNetworkInfo;
                networkInfo.LastBroadcast = DateTime.Now.ToString("MM/dd/yyyy - HH:mm:ss");
                MonitorMap.AddOrUpdate(networkInfo);
                _loger.WriteEvent(string.Format("Receive broadcast from {0}",networkInfo.RemoteIp));
            }
            catch (Exception ex)
            {
                _loger.WriteError("Error: Listening Monitor broadcast error: ",ex);
            }
            finally
            {
                this.TurnOnTimer();
            }
            
        }
    }
}