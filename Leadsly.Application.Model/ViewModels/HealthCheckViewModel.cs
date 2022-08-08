using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class HealthCheckViewModel
    {
        [DataMember(Name = "apiVersion")]
        public string? APIVersion { get; set; }

        [DataMember(Name = "name")]
        public string? Name { get; set; }
    }
}
