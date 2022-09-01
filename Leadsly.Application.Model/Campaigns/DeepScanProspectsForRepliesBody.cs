﻿using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class DeepScanProspectsForRepliesBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; } = string.Empty;
        [DataMember]
        public string LeadslyUserFullName { get; set; } = string.Empty;
    }
}
