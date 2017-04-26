using System;
using System.IO;
using Wiki.Api.Help.Controllers.Help;
using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget.AssemblyInfo
{
    internal class XmlPartOfAssembly : IPartOfAssembly
    {
        private const string FILEEXTENSION = ".xml";
        public string PathOfPart { get; set; }

        public static XmlPartOfAssembly CreateWithAssemblyName(string assemblyName)
        {
            var instance = new XmlPartOfAssembly
            {
                PathOfPart = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + FILEEXTENSION)
            };
            return instance;
        }
        public static XmlPartOfAssembly CreateWithPath(string path)
        {
            var instance = new XmlPartOfAssembly
            {
                PathOfPart = path
            };
            return instance;
        }
        public void SaveInFileSystem(AssemblyContent assemblyContent)
        {
            if (assemblyContent.XmlContent != null)
            {
                var path = Path.Combine(PathOfPart);
                File.WriteAllBytes(path, assemblyContent.XmlContent);
                HelpHelper.AddXml(assemblyContent.Name, path);
            }
        }

        public void DeleteInFileSystem(AssemblyContent assemblyContent)
        {
            
        }
    }
}
