using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.RabbitMQ
{
    [DataContract]
    public class TriggerFollowUpMessageBody
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string HalId { get; set; }
    }
}
