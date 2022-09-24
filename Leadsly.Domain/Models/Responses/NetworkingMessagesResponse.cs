using Leadsly.Domain.MQ.Messages;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Responses
{
    [DataContract]
    public class NetworkingMessagesResponse
    {
        [DataMember]
        public IList<NetworkingMessageBody> Items { get; set; }
    }
}
