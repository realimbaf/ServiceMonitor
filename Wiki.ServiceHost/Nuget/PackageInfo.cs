using System;
using System.Text;
using System.Xml;

namespace Wiki.ServiceHost.Nuget
{
    [Serializable]
    public class PackageInfo
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string NugetFile { get; set; }
        public static PackageInfo Parse(byte[] xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(Encoding.UTF8.GetString(xml));
            var info = new PackageInfo
            {
                Id = GetValue(doc, "id"),
                Description = GetValue(doc, "description"),
                Version = GetValue(doc, "version"),
                Title = GetValue(doc, "title"),
            };
            
            return info;
        }

        private static string GetValue(XmlDocument doc, string key)
        {
            var tags = doc.GetElementsByTagName(key);
            if (tags.Count > 0)
                return tags[0].InnerXml;
            return null;
        }
    }
}