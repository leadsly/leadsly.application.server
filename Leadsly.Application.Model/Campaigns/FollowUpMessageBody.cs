﻿using System;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class FollowUpMessageBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string ProspectName { get; set; }

        [DataMember]
        public string ProspectProfileUrl { get; set; }

        [DataMember]
        public string CampaignProspectId { get; set; }

        [DataMember]
        public string FollowUpMessageId { get; set; }

        [DataMember]
        public int OrderNum { get; set; }

        public DateTimeOffset ExpectedDeliveryDateTime { get; set; }

    }

}
