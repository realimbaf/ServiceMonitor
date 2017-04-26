using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget.DomainBuilder
{
    internal class DomainBuilder : IDomainBuilder
    {
        private readonly string _id;
        private readonly string _configFile;
        private readonly ICollection<AssemblyContent> _assemblies;
        private AppDomain _domain;

        public DomainBuilder(ICollection<AssemblyContent> assemblies,string id,string configFile)
        {
            _id = id;
            _configFile = configFile;
            _assemblies = assemblies;
        }

        public void BuildDirectories(string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
            catch (Exception e)
            {
                MainSrv.Logger.WriteError("Error copy host.", e);

            }
        }

        public AppDomainSetup BuildDomainSetup(string domainDir)
        {
            return new AppDomainSetup
            {
                ApplicationName = "tmp_App",
                CachePath = domainDir,
                DynamicBase = Path.Combine(domainDir, "tmp"),
                ShadowCopyFiles = "true",
                LoaderOptimization = LoaderOptimization.SingleDomain,
                ApplicationBase = domainDir
            };
        }

        public void BuildConfigs(AppDomainSetup setup,string dir)
        {
            if (!string.IsNullOrWhiteSpace(_configFile))
            {
                var configPath = Path.Combine(dir, _id + ".config");
                setup.ConfigurationFile = configPath;
                File.WriteAllText(configPath, _configFile);
            }
        }
        public AppDomain GetResult(AppDomainSetup setup)
        {
            var securityPolicy = AppDomain.CurrentDomain.Evidence;
            _domain = AppDomain.CreateDomain("host_" + _id, securityPolicy, setup);
            var obj = _domain.CreateInstanceFrom(typeof(DomainBuilder).Assembly.Location, "Wiki.ServiceHost.ServiceHost");

            var host = (ServiceHost)obj.Unwrap();
            host.InitEvents(_domain);

            LoadAssembly();
            return _domain;
        }
        private void LoadAssembly()
        {
            var assemblyContents = _assemblies
               .Where(x => x.Name != "Wiki.Service.Configuration")
               .ToList();

            var obj = _domain.CreateInstance("Wiki.ServiceHost", "Wiki.ServiceHost.Nuget.AssemblyLoader");
            var loader = (AssemblyLoader)obj.Unwrap();

            foreach (var assembly in assemblyContents)
            {
                if (assembly.Content != null)
                    loader.Load(assembly);
            }
        }

    }
}
