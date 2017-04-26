using System;
using System.IO;
using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget.AssemblyInfo
{
    internal class PdbPartOfAssembly : IPartOfAssembly
    {
        private const string FILEEXTENSION = ".pdb";
        public string PathOfPart { get; set; }
        public static PdbPartOfAssembly CreateWithAssemblyName(string assemblyName)
        {
            var instance = new PdbPartOfAssembly
            {
                PathOfPart = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName + FILEEXTENSION)
            };
            return instance;
        }
        public static PdbPartOfAssembly CreateWithPath(string path)
        {
            var instance = new PdbPartOfAssembly
            {
                PathOfPart = path
            };
            return instance;
        }

        public void SaveInFileSystem(AssemblyContent assemblyContent)
        {
            if (assemblyContent.PdbContent != null)
                File.WriteAllBytes(Path.Combine(PathOfPart), assemblyContent.PdbContent);
        }

        public void DeleteInFileSystem(AssemblyContent assemblyContent)
        {
            var pdbFile = Path.Combine(PathOfPart);
            if (File.Exists(pdbFile))
            {
                File.Delete(pdbFile);
            }
        }
    }
}
