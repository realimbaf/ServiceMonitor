using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget.AssemblyInfo
{
    /// <summary>
    /// Represents interface for loading assembly.
    /// </summary>
    internal interface ILoader
    {
        /// <summary>
        /// Method that is loading assembly.
        /// </summary>
        /// <param name="assemblyContent"></param>
        void Load(AssemblyContent assemblyContent);
    }
}
