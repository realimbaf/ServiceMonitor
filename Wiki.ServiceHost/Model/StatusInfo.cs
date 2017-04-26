using System;
using System.Diagnostics;

namespace Wiki.ServiceHost.Model
{
    public class StatusInfo
    {
        public int BasePriority { get; set; }
        public int ProccessId { get; set; }
        public string MachineName { get; set; }
        public long PagedMemorySize64 { get; set; }
        public long PagedSystemMemorySize64 { get; set; }
        public long PeakPagedMemorySize64 { get; set; }
        public long PeakVirtualMemorySize64 { get; set; }
        public long PeakWorkingSet64 { get; set; }
        public long PrivateMemorySize64 { get; set; }
        public string ProcessName { get; set; }
        public TimeSpan PrivilegedProcessorTime { get; set; }
        public int SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan TotalProcessorTime { get; set; }
        public TimeSpan UserProcessorTime { get; set; }
        public long VirtualMemorySize64 { get; set; }
        public long WorkingSet64 { get; set; }
        public string[] Assemblies { get; set; }
        public string NugetFile { get; set; }
        public string ServiceId { get; set; }
    }
}