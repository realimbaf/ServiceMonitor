using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Wiki.ServiceHost.Helpers;
using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget
{
    public class NugetPackage
    {
        private PackageInfo _info;
        private readonly List<AssemblyContent> _assemblies = new List<AssemblyContent>();
        private readonly AppDomain _domain;
        private string _configFile;

        private NugetPackage()
        {
            this._domain = AppDomain.CurrentDomain;
            this._domain.AssemblyResolve += AssembluResolve;
            
        }

        private Assembly AssembluResolve(object sender, ResolveEventArgs args)
        {
            var ass = _assemblies.FirstOrDefault(x => x.Assembly.FullName == args.Name);
            return ass != null ? ass.Assembly : null;
        }

        public PackageInfo Info
        {
            get { return this._info; }
        }

        internal string ConfigFile { get { return this._configFile; } }

        internal ICollection<AssemblyContent> Assemblies { get { return this._assemblies.ToList(); } }
       
        private void Unzip(byte[] content)
        {
            using (var zipStream = new ZipInputStream(new MemoryStream(content)))
            {
                ZipEntry theEntry;
                while ((theEntry = zipStream.GetNextEntry()) != null)
                {
                    if (theEntry.Name.EndsWith("nuspec", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var nContent = ReadContent(zipStream);
                        this._info = PackageInfo.Parse(nContent);
                        
                    }
                    if (theEntry.Name.EndsWith("config", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var nContent = ReadContent(zipStream);
                        this._configFile = Encoding.UTF8.GetString(nContent);

                    }
                    else if (Regex.IsMatch(theEntry.Name, "^lib/net45/.*\\.(dll)|(pdb)|(xml)", RegexOptions.IgnoreCase))
                    {
                        var file = ReadContent(zipStream);
                        LoadAssembly(file, theEntry.Name);
                    }
                }

            }
        }
        private void LoadAssembly(byte[] file, string name)
        {
            var assemblyFileName = Path.GetFileNameWithoutExtension(name);
            var assemblyExtension = (Path.GetExtension(name) ?? "").ToLower();
            var content = _assemblies.FirstOrDefault(x => x.Name == assemblyFileName);

            if (content == null)
            {
                content = new AssemblyContent {Name = assemblyFileName};
                _assemblies.Add(content);
            }
            switch (assemblyExtension)
            {
                case ".dll":
                    content.Content = file;
                    break;
                case ".pdb":
                    content.PdbContent = file;
                    break;
                default:
                    content.XmlContent = file;
                    break;
            }
        }
        internal static NugetPackage LoadPackage(ArgumentParser parser)
        {
            try
            {
                var pkg = new NugetPackage();
                var version = parser.Version;
                if (string.IsNullOrWhiteSpace(version))
                {
                    version = GetLastVersion(parser.NugetDir, parser.Id);
                }                   
                var fileName = string.Format("{0}.{1}.nupkg", parser.Id, version);
                var filePath = Path.Combine(parser.NugetDir, fileName);
                var content = File.ReadAllBytes(filePath);
                pkg.Unzip(content);
                pkg.Info.NugetFile = fileName;
                return pkg;
            }

            catch (NugetNoadException)
            {
                throw;
            }
            catch (Exception er)
            {
                throw new NugetNoadException("Error load package." + er.Message, er);
            }
        }


        private static byte[] ReadContent(Stream stream)
        {
            var buf = new byte[stream.Length];
            stream.Read(buf, 0, buf.Length);
            return buf; 
        }
        private static string GetLastVersion(string repositoryPath, string id)
        {
            var fileMask = string.Format("{0}.*.nupkg", id);
            var dir = new DirectoryInfo(repositoryPath);
            var names = dir.GetFiles(fileMask).Select(x => x.Name).ToArray();
            var reg = names.Select(x => Regex.Match(x, "^" + id + "\\.(?<ver>\\d(\\.\\d+){2,4})\\.nupkg").Groups["ver"]).Where(x => x.Success);
            var ver = reg.Select(x => new { x.Value, v = new Version(x.Value) }).OrderByDescending(x => x.v).FirstOrDefault();
            if (ver == null)
                throw new NugetNoadException(string.Format("Not found nuget package:{0}, dir:{1}", id, repositoryPath));
            return ver.Value;
        }

    }
     

}