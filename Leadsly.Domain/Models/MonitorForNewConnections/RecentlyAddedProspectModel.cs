using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.MonitorForNewConnections
{
    [DataContract]
    public class RecentlyAddedProspectModel
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ProfileUrl { get; set; }
        [DataMember]
        public long AcceptedRequestTimestamp { get; set; }
    }
}
