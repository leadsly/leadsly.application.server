using System.Runtime.Serialization;

namespace Leadsly.Models.ViewModels
{
    [DataContract]
    public class HealthCheckViewModel
    {
        [DataMember(Name ="apiVersion")]
        public string APIVersion { get; set; }
    }
}
