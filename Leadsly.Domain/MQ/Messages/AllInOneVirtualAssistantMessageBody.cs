using System.Runtime.Serialization;

namespace Leadsly.Domain.MQ.Messages
{
    [DataContract]
    public class AllInOneVirtualAssistantMessageBody : PublishMessageBody
    {
        [DataMember(IsRequired = false)]
        public DeepScanProspectsForRepliesBody DeepScanProspectsForReplies { get; set; }

        [DataMember]
        public CheckOffHoursNewConnectionsBody CheckOffHoursNewConnections { get; set; }

    }
}
