using Wiki.ServiceHost.Model;

namespace Wiki.ServiceHost.Nuget.AssemblyInfo
{
    /// <summary>
    /// Represents the description the part of Assembly
    /// </summary>
    internal interface IPartOfAssembly
    {
        /// <summary>
        /// Property to store file extension.
        /// </summary>
        string PathOfPart { get; set; }
        /// <summary>
        /// Method of saving of the Assembly on disk.
        /// </summary>
        void SaveInFileSystem(AssemblyContent assemblyContent);

        /// <summary>
        /// Method of deleting of the Assembly on disk.
        /// </summary>
        void DeleteInFileSystem(AssemblyContent assemblyContent);

    }
}
