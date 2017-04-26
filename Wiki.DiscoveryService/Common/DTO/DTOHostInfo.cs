namespace Wiki.DiscoveryService.Common.DTO
{
    public class DTOHostInfo
    {
        public string Hostname { get; set; }
        public string RemoteIp { get; set; }
        public string  MonitorUrl { get; set; }
        public string LastBroadcast { get; set; }
        public string LastUpdate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDebug { get; set; }
        public bool IsDirect { get; set; }
    }
}