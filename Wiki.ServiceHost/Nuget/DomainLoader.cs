using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wiki.ServiceHost.Model;
using Wiki.ServiceHost.Nuget.DomainBuilder;

namespace Wiki.ServiceHost.Nuget
{
    [Serializable]
    internal class DomainLoader
    {
        private readonly ICollection<AssemblyContent> _assemblies;
        private  IDomainBuilder _builder;
        public DomainLoader(ICollection<AssemblyContent> assemblies)
        {
            this._assemblies = assemblies;          
        }

        public AppDomain CreateDomain(string id, string configFile)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, id);
            _builder = new DomainBuilder.DomainBuilder(_assemblies,id,configFile);

            _builder.BuildDirectories(dir);
            var assemblySetup = _builder.BuildDomainSetup(dir);
            _builder.BuildConfigs(assemblySetup,dir);
            return _builder.GetResult(assemblySetup);
        }
    }
}