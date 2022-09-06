using Leadsly.Domain.Models.FollowUpMessage;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class SentFollowUpMessageRequest
    {
        [DataMember]
        public SentFollowUpMessageModel Item { get; set; }
    }
}
