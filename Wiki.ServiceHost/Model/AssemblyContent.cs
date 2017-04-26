using System;
using System.Reflection;

namespace Wiki.ServiceHost.Model
{
    internal class AssemblyContent : MarshalByRefObject
    {
        public string Name { get; set; }

        public byte[] Content { get; set; }
        public byte[] XmlContent { get; set; }
        public byte[] PdbContent { get; set; }

        public Assembly Assembly { get; set; }
    }
}
