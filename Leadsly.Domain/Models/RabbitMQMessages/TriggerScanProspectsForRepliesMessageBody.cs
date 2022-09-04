using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class TriggerScanProspectsForRepliesMessageBody : TriggerPhaseMessageBodyBase
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string HalId { get; set; }
    }
}
