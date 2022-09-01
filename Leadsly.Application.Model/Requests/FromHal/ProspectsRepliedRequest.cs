using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class ProspectsRepliedRequest : BaseHalRequest
    {
        [DataMember(Name = "ProspectsReplied", IsRequired = false)]
        public IList<ProspectRepliedRequest> Items { get; set; }
    }
}
