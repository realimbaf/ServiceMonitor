using System;
using System.Collections.Generic;
using System.Reflection;
using Wiki.ServiceHost.Model;
using Wiki.ServiceHost.Nuget.AssemblyInfo;

namespace Wiki.ServiceHost.Nuget
{
    /// <summary>
    /// 
    /// </summary>
    internal class AssemblyLoader : MarshalByRefObject,ILoader
    {
        private  IList<IPartOfAssembly> _partsOfAssembly;

        public void Load(AssemblyContent assemblyContent)
        {
            _partsOfAssembly = new List<IPartOfAssembly>()
            {
                DllPartOfAssembly.CreateWithAssemblyName(assemblyContent.Name),
                PdbPartOfAssembly.CreateWithAssemblyName(assemblyContent.Name),
                XmlPartOfAssembly.CreateWithAssemblyName(assemblyContent.Name)
            };
            SaveFiles(assemblyContent);
            Assembly.LoadFrom(_partsOfAssembly[0].PathOfPart);  //Loading Dll.
            DeleteFiles(assemblyContent);
        }

        private void DeleteFiles(AssemblyContent content)
        {
            foreach (var part in _partsOfAssembly)
            {
                part.DeleteInFileSystem(content);
            }
        }

        private void SaveFiles(AssemblyContent content)
        {
            foreach (var part in _partsOfAssembly)
            {
                part.SaveInFileSystem(content);
            }
        }

    }
}