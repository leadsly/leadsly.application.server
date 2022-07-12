using Leadsly.Application.Model.ViewModels.Campaigns;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.Models.Requests
{
    [DataContract]
    public class CreateCampaignRequest
    {
        [DataMember(Name = "campaignDetails", IsRequired = true)]
        public CampaignDetailsViewModel CampaignDetails { get; set; }

        [DataMember(Name = "messages", IsRequired = true)]
        public List<FollowUpMessageViewModel> FollowUpMessages { get; set; }

        [DataMember(Name = "connectedAccount", IsRequired = true)]
        public string ConnectedAccount { get; set; } = string.Empty;

        [DataMember(Name = "halId")]
        public string HalId { get; set; } = string.Empty;
    }
}
