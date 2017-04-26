using System;

namespace Wiki.ServiceHost.Helpers
{
    /// <summary>
    /// Класс ошибки загрузки nuget пакета
    /// </summary>
    public class NugetNoadException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public NugetNoadException(string message) : base(message) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public NugetNoadException(string message, Exception inner) : base(message, inner) { }
    }
}
