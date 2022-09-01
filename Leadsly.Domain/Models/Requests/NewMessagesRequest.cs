using System.Collections.Generic;

namespace Leadsly.Domain.Models.Requests
{
    public class NewMessagesRequest
    {
        public IList<NewMessageRequest> Items { get; set; }
    }
}
