using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Microsoft.Owin.Hosting;
using Wiki.Service.Common.Clients;
using Wiki.Service.Configuration;
using Wiki.ServiceHost.Nuget;

namespace Wiki.ServiceHost
{
    public class MainSrv:IDisposable
    {
        public static FileLogger _logger=new FileLogger("host_"+Process.GetCurrentProcess().Id);
        private readonly ManualResetEventSlim _stopSwitch = new ManualResetEventSlim();

        private static MainSrv _instanse;

        private readonly string _url;
        private IDisposable _srv;
        private static List<IServiceConfig> _configs;
        private MonitorPulling _pulling;
        private static int _port;

        private bool _isStoping;
        private static PackageInfo _info;

        public MainSrv()
        {
            if (_port == 0)
                _port = GetFreeTcpPort();
            this._url = string.Format("http://+:{0}/",_port);
            _instanse = this;

        }


        static int GetFreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public string Url
        {
            get { return this._url; }
        }



        public static PackageInfo PackageInfo { get { return _info; } }

        public static FileLogger Logger
        {
            get { return _logger; }
        }


        public void Start()
        {
            _logger.WriteEvent("*****Service start******");

            MainSrv._logger.WriteEvent("*****Start backgraund proces******");

            foreach (var config in _configs)
            {
                try
                {
                    config.Start();
                }
                catch (Exception e)
                {
                    MainSrv._logger.WriteError("Error start backgraund proces", e);
                }
            }

            StartWeb();
            this._pulling = new MonitorPulling(PackageInfo, this._url, Logger);
            this._pulling.Start();
        }

        private void StartWeb()
        {
            _logger.WriteEvent("Service start. url:{0}", this._url);
            _srv = WebApp.Start<Startup>(url: this._url);


        }

        public void Stop()
        {
            if(this._isStoping)
                return;
            this._isStoping = true;
            _logger.WriteEvent("*****Stop web******");
            ShutDownMiddleware.ShutDown();
            while (ShutDownMiddleware.GetRequestCount() != 0)
            {
                MainSrv._logger.WriteEvent("Requesrts count:{0}", ShutDownMiddleware.GetRequestCount());
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            _logger.WriteEvent("*****Stop backgraund proces******");

            foreach (var config in _configs)
            {
                try
                {
                    config.Stop();
                }
                catch (Exception e)
                {
                    MainSrv._logger.WriteError("Error stop backgraund proces",e);
                }
            }

            if(_srv != null)
                _srv.Dispose();
            if(this._pulling != null)
                this._pulling.Stop();

            _logger.WriteEvent("*****Service stop******");
            _stopSwitch.Set();

        }

        public static void StopAsync()
        {
            if(_instanse!=null)
            Task.Delay(100).ContinueWith(t => _instanse.Stop());
        }

        public void Wait()
        {
            _stopSwitch.Wait();
        }

        public void Dispose()
        {
            Stop();
        }

        public static void LoadPackage(string[] args)
        {
            var parser = new ArgumentParser(args);
            parser.Parse();
            _port = parser.Port;
            
            NugetPackage.LoadPackage(parser);
            LoadConfigs();
        }

        private static void LoadConfigs()
        {
            var type = typeof (IServiceConfig);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().OrderBy(x=>x.FullName).ToArray();
            var typeList = assemblies.SelectMany(x => x.GetTypes()).ToList();
            var types = typeList
                .Where(x=>!x.IsAbstract&&!x.IsInterface)
                .Where(x => type.IsAssignableFrom(x)).ToList();
            _configs = types.Select(CreateServiceConfig).ToList();
            Startup.LoadConfig(_configs);
        }

        private static IServiceConfig CreateServiceConfig(Type type)
        {
            var ctor = Expression.New(type);
            var item = Expression.Lambda<Func<IServiceConfig>>(ctor).Compile()();
            return item;
        }



        internal static void RunService(ArgumentParser parser, PackageInfo info)
        {
            FileLogger.SetFilePrefix(parser.Id);
            FileLogger.SetBaseDir(Environment.CurrentDirectory);
            ConfigurationContainer.SetBaseDir(Environment.CurrentDirectory);

            DiscoveryFactory.SetDefaultCreditenals(new ClientCreditenals { ClientId = "service", ClientSecret = "servicepasswd" });


            MainSrv.Logger.WriteEvent(string.Format("Loaded package: {0}, version:{1}({2}).\nAssemblies:\n{3}", info.Id, info.Version, info.NugetFile,
    string.Join(Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.FullName).Select(x => x.FullName))));


            ConfigurationContainer.SetId(parser.Id);
            MainSrv.Logger.WriteEvent("Use configuration file:{0}", ConfigurationContainer.Configuration.GetConfigPath());


            _port = parser.Port;

            _info = info;

            LoadConfigs();

            using (var service = new MainSrv())
            {
                service.Start();
                service.Wait();
            }

        }
    }
}