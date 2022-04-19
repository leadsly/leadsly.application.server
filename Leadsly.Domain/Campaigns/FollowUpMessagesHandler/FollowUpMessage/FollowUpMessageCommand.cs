using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessage
{
    public class FollowUpMessageCommand : ICommand
    {
        public FollowUpMessageCommand(string campaignId, string campaignProspectFollowUpMessageId, DateTimeOffset scheduleTime)
        {
            CampaignId = campaignId;
            CampaignProspectFollowUpMessageId = campaignProspectFollowUpMessageId;
            ScheduleTime = scheduleTime;
        }

        public string CampaignId { get; set; }
        public string CampaignProspectFollowUpMessageId { get; set; }
        public DateTimeOffset ScheduleTime { get; set; }
    }
}
