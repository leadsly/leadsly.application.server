namespace Leadsly.Domain.Models.Entities
{
    public class CloudMapServiceDiscoveryConfig
    {
        public CloudMapConfig Grid { get; set; }
        public CloudMapConfig Hal { get; set; }
        public CloudMapConfig AppServer { get; set; }
        public CloudMapConfig Proxy { get; set; }
    }

    public class CloudMapConfig
    {
        public string Name { get; set; }
        public string NamespaceId { get; set; }
        public int DnsRecordTTL { get; set; }
        public string DnsRecordType { get; set; }
    }
}
