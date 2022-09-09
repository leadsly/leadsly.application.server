﻿using System;
using System.Runtime.Serialization;

namespace Leadsly.Domain.MQ.Messages
{
    [DataContract]
    public class NetworkingMessageBody : PublishMessageBody
    {
        /// <summary>
        /// Represents the number of prospects from the search url this phase should crawl.
        /// Should never be greater than 10 because that is the max number of prospects
        /// displayed on search result hitlists
        /// </summary>
        [DataMember]
        public int ProspectsToCrawl { get; set; }

        [DataMember]
        public string PrimaryProspectListId { get; set; } = string.Empty;

        [DataMember]
        public string CampaignId { get; set; } = string.Empty;

        [DataMember]
        public string CampaignProspectListId { get; set; } = string.Empty;

        [DataMember]
        public string StartTime { get; set; } = string.Empty;

        [DataMember]
        public int FailedDeliveryCount { get; set; }

        [DataMember]
        public string SocialAccountId { get; set; } = string.Empty;

        public DateTimeOffset NowLocalized { get; set; }

    }
}
