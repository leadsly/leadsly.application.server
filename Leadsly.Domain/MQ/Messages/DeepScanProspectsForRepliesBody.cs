using System.Runtime.Serialization;

namespace Leadsly.Domain.MQ.Messages
{
    [DataContract]
    public class DeepScanProspectsForRepliesBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; } = string.Empty;
    }
}
