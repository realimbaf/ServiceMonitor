using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CarParts.Common.Log;
using Wiki.ServiceHost.Nuget;
using Encoding = System.Text.Encoding;

namespace Wiki.ServiceHost
{
    internal class Program
    {
        internal const string StartMessage = "***** Start service host ******{0}{0}Process id:{1}{0}Arguments:[{2}]{0}{0}*******************************";

        static void Main(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif
            FileLogger.SetFilePrefix("host");
            AppDomain.CurrentDomain.UnhandledException += DomainUnhandledException;
            var startMessage = string.Format(StartMessage, Environment.NewLine, Process.GetCurrentProcess().Id,
                string.Join(" ", args));
            MainSrv.Logger.WriteEvent(startMessage);
            PrintEnvironment();

            try
            {

                var parser = new ArgumentParser(args);
                parser.Parse();
                FileLogger.SetFilePrefix(parser.Id);
                MainSrv.Logger.WriteEvent(startMessage);
                PrintEnvironment();     

                ServiceHost.Start(args);

            }
            catch (Exception e)
            {
                MainSrv.Logger.WriteError("Error start service.",e);
                Thread.Sleep(1000);
            }

        }

        internal static void DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MainSrv.Logger.WriteError("UnhandledException",(Exception) e.ExceptionObject);
        }

        internal static void PrintEnvironment()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("OSVersion:{1}{0}", Environment.NewLine, Environment.OSVersion);
            sb.AppendFormat("MachineName:{1}{0}", Environment.NewLine, Environment.MachineName);
            sb.AppendFormat("Is64BitProcess:{1}{0}", Environment.NewLine, Environment.Is64BitProcess);
            sb.AppendFormat("BaseDirectory:{1}{0}", Environment.NewLine, AppDomain.CurrentDomain.BaseDirectory);
            sb.AppendFormat("ExecutingAssemblyLocation:{1}{0}", Environment.NewLine, Assembly.GetExecutingAssembly().Location);
            sb.AppendFormat("CurrentDirectory:{1}{0}", Environment.NewLine, Environment.CurrentDirectory);
            sb.AppendLine("Assembly:" +Environment.NewLine+
                          string.Join(Environment.NewLine, AppDomain.CurrentDomain.GetAssemblies().Select(x => x.ToString())));
            MainSrv.Logger.WriteEvent("Environment:{0}{1}", Environment.NewLine, sb.ToString());
        }
    }


    public static class NetAclChecker
    {
        public static void AddAddress(string address)
        {
            AddAddress(address, Environment.UserDomainName, Environment.UserName);
        }

        public static void DeleteAddress(string address)
        {
            var args = string.Format(@"http delete urlacl url={0}", address);

            var psi = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.GetEncoding(866)
            };
            var p = Process.Start(psi);
            p.WaitForExit();
            var str = p.StandardOutput.ReadToEnd();
            File.WriteAllText(@"c:\temp\1.log",str);
            Console.WriteLine(str);
        }

        public static void AddAddress(string address, string domain, string user)
        {
            var args = string.Format(@"http add urlacl url={0} user=\Все", address, domain, user);

            var psi = new ProcessStartInfo("netsh", args)
            {
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.GetEncoding(866)
            };
            var p = Process.Start(psi);
            p.WaitForExit();
            var str = p.StandardOutput.ReadToEnd();
            File.WriteAllText(@"c:\temp\1.log", str);
            Console.WriteLine(str);
        }
    }
}
