using Leadsly.Domain.Models.ScanProspectsForReplies;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class NewMessagesRequest
    {
        [DataMember]
        public IList<NewMessageModel> Items { get; set; }
    }
}
