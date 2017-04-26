using System;
using System.Threading;
using CarParts.Common.Log;

namespace Wiki.Service.Configuration
{
    public abstract class SchedulerBase
    {
        protected readonly FileLogger _logger;
        private Timer _timer;
        protected int _interval = 60000;

        protected SchedulerBase(FileLogger logger)
        {
            this._logger = logger;
        }

        public void Start()
        {
            this._logger.WriteEvent("Scheduler start " + this.GetType().Name);
            var interval = this._interval;
            this._interval = 1000;
            this.TurnOnTimer();
            this._interval = interval;

        }

        public void Stop()
        {
            this._logger.WriteEvent("Scheduler stop " + this.GetType().Name);
            if (this._timer != null)
                this._timer.Change(-1, -1);
            this._timer = null;
        }

        private void TurnOnTimer()
        {
            if (this._timer == null)
                this._timer = new Timer(TimerProcess, null, this._interval, Timeout.Infinite);
            else
                this._timer.Change(this._interval, Timeout.Infinite);
        }

        private void TimerProcess(object state)
        {
            try
            {
                MainProcess();
            }
            catch (Exception e)
            {
                this._logger.WriteError("Error in scheduler " + this.GetType().Name, e);
            }
            finally
            {
                TurnOnTimer();
            }
        }

        protected abstract void MainProcess();
    }
}