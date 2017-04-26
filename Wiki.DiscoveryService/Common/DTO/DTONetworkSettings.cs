namespace Wiki.DiscoveryService.Common.DTO
{
    public class DTONetworkSettings
    {
        public string HostName { get; set; }
        public string MonitorUrl { get; set; }
        public string RemoteIp  { get; set; }
        public bool IsDebug { get; set; }
        public bool IsDirect { get; set; }
    }
}