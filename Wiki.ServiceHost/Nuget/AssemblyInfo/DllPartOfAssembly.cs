using System;
using System.IO;
using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget.AssemblyInfo
{
    internal class DllPartOfAssembly : IPartOfAssembly
    {
        private const string FILEEXTENSION = ".dll";

        private DllPartOfAssembly()
        {
            
        }
        public static DllPartOfAssembly CreateWithAssemblyName(string assemblyName)
        {
            var instance = new DllPartOfAssembly
            {
                PathOfPart = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + FILEEXTENSION)
            };
            return instance;
        }
        public static DllPartOfAssembly CreateWithPath(string path)
        {
            var instance = new DllPartOfAssembly
            {
                PathOfPart = path
            };
            return instance;
        }

        public string PathOfPart { get; set; }

        public void SaveInFileSystem(AssemblyContent assemblyContent)
        {
            try
            {
                File.WriteAllBytes(PathOfPart, assemblyContent.Content);
            }
            catch (Exception ex)
            {
                
                throw;
            }        
        }
        public void DeleteInFileSystem(AssemblyContent assemblyContent)
        {
            File.Delete(PathOfPart);
        }
    }
}
