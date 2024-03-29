﻿using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class TriggerFollowUpMessageBody : TriggerPhaseMessageBodyBase
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string HalId { get; set; }
    }
}
