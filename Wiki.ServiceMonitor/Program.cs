using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Wiki.Service.Configuration;

namespace Wiki.ServiceMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            FileLogger.SetFilePrefix("monitoring");
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            MonitorSrv.Logger.WriteEvent("******* Start service monitoring **********");
            PrintEnvironment();
            ConfigurationContainer.SetId("ServiceMonitoring");
            MonitorSrv.Logger.WriteEvent("Use configuration file:{0}", ConfigurationContainer.Configuration.GetConfigPath());
            var service = new MonitorService();
            if (Environment.UserInteractive)
            {
                MonitorSrv.Logger.LoggerWrite(LoggerEventType.Event, "Event");
                MonitorSrv.Logger.LoggerWrite(LoggerEventType.Warning, "Warning");
                MonitorSrv.Logger.LoggerWrite(LoggerEventType.Error, "Error");
                service.OnStart();
                Console.ReadKey();
                service.Stop();

            }
            else
            {
                ServiceBase.Run(service);
            }


        }

        static void PrintEnvironment()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("BaseDirectory:{1}{0}",Environment.NewLine, AppDomain.CurrentDomain.BaseDirectory);
            sb.AppendFormat("ExecutingAssemblyLocation:{1}{0}", Environment.NewLine, Assembly.GetExecutingAssembly().Location);
            sb.AppendFormat("CurrentDirectory:{1}{0}", Environment.NewLine, Environment.CurrentDirectory);
            MonitorSrv.Logger.WriteEvent("Environment:{0}{1}", Environment.NewLine,sb.ToString());
            
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MonitorSrv.Logger.WriteError("UnhandledException", (Exception)e.ExceptionObject);
        }



    }
    public static class NetAclChecker
    {

        public static void DeleteAddress(string address)
        {
            string args = string.Format(@"http delete urlacl url={0}", address);

            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.StandardOutputEncoding = Encoding.GetEncoding(866);
            var p = Process.Start(psi);
            //Console.OutputEncoding.Dump();
            p.WaitForExit();
            var str = p.StandardOutput.ReadToEnd();
            MonitorSrv.Logger.WriteEvent("Delete urlacl:{0}", str);
        }

        public static void AddAddress(string address)
        {
            string args = string.Format(@"http add urlacl url={0} user=\Все", address);

            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.StandardOutputEncoding = Encoding.GetEncoding(866);
            var p = Process.Start(psi);
            //Console.OutputEncoding.Dump();
            p.WaitForExit();
            var str = p.StandardOutput.ReadToEnd();
            MonitorSrv.Logger.WriteEvent("Check urlacl:{0}", str);
        }
    }

}
