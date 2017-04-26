using Newtonsoft.Json;

namespace Wiki.ServiceMonitor.Models
{
    public class ActiveService
    {
        public string Id { get; set; }

        [JsonIgnore]
        public bool IsManualStop { get; set; }

        public string Version { get; set; }


    }
}