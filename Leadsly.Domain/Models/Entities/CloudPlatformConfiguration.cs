namespace Leadsly.Domain.Models.Entities
{
    public class CloudPlatformConfiguration
    {
        public string Region { get; set; }
        public string ApiServiceDiscoveryName { get; set; }
        public EcsServiceConfig EcsServiceConfig { get; set; }
        public EcsTaskConfig EcsTaskConfig { get; set; }
        public EcsTaskDefinitionConfig EcsProxyTaskDefinitionConfig { get; set; }
        public EcsTaskDefinitionConfig EcsHalTaskDefinitionConfig { get; set; }
        public EcsTaskDefinitionConfig EcsGridTaskDefinitionConfig { get; set; }
        public CloudMapServiceDiscoveryConfig ServiceDiscoveryConfig { get; set; }
    }
}
