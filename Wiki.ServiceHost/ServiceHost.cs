using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Wiki.ServiceHost.Nuget;

namespace Wiki.ServiceHost
{
    public class ServiceHost : MarshalByRefObject
    {
        private readonly string[] _args;
        private readonly ArgumentParser _parser;


        public static void Start(string[] args)
        {
            var parser = new ArgumentParser(args);
            parser.Parse();
            var package = NugetPackage.LoadPackage(parser);

            var loader = new DomainLoader(package.Assemblies);
            var domain = loader.CreateDomain(parser.Id,package.ConfigFile);


            var obj = domain.CreateInstance("Wiki.ServiceHost", "Wiki.ServiceHost.ServiceHost");
            var host = (ServiceHost)obj.Unwrap();

            host.Run(parser,package.Info);
            var dir = Path.Combine(domain.SetupInformation.ApplicationBase,domain.SetupInformation.ApplicationName);
            AppDomain.Unload(domain);
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception e)
            {
               //TODO: ignored exception
            }
        }

        internal void Run(ArgumentParser parser, PackageInfo info)
        {                  
            MainSrv.RunService(parser, info);         
        }

        internal void InitEvents(AppDomain domain)
        {
            domain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            domain.AssemblyLoad += domain_AssemblyLoad;
            domain.ReflectionOnlyAssemblyResolve += domain_ReflectionOnlyAssemblyResolve;
            domain.TypeResolve += domain_TypeResolve;
            domain.UnhandledException += Program.DomainUnhandledException;
        }
        //TODO : what is the methods?
        private static Assembly domain_TypeResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        private static Assembly domain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var a = args.Name;
            return null;
        }

        private static void domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.FullName).ToArray();
            var c = args.LoadedAssembly;
        }


        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var name = new AssemblyName(args.Name);
            var ass = assemblies.FirstOrDefault(x => x.GetName().Name == name.Name);
            if (ass == null)
            {
                var fn = Path.Combine(Environment.CurrentDirectory, name.Name + ".dll");
                if (File.Exists(fn))
                {

                    ass=Assembly.LoadFile(fn);
                }
            }
            return ass;

        }
    }
}