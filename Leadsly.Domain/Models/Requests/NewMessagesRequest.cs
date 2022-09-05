﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class NewMessagesRequest
    {
        [DataMember]
        public IList<NewMessage> Items { get; set; }
    }
}
