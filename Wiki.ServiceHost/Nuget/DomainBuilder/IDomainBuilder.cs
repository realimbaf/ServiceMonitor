using System;
using System.Collections.Generic;

namespace Wiki.ServiceHost.Nuget.DomainBuilder
{
    interface IDomainBuilder
    {
        void BuildDirectories(string dir);
        AppDomainSetup BuildDomainSetup(string dir);
        void BuildConfigs(AppDomainSetup setup, string dir);
        AppDomain GetResult(AppDomainSetup setup);

    }
}
