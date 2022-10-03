using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<bool> ProcessSentFollowUpMessageAsync(string campaignProspectId, SentFollowUpMessageRequest request, CancellationToken ct = default)
        {
            CampaignProspect campaignProspect = await _campaignRepositoryFacade.GetCampaignProspectByIdAsync(campaignProspectId, ct);

            if (campaignProspect == null)
            {
                _logger.LogError("Failed to retrieve CampaignProspect by {campaignProspectId}", campaignProspectId);
                return false;
            }

            CampaignProspectFollowUpMessage followUp = campaignProspect.FollowUpMessages.FirstOrDefault(x => x.Order == request.Item.MessageOrderNum);

            followUp.ActualDeliveryDateTimeStamp = request.Item.ActualDeliveryDateTimeStamp;
            campaignProspect.FollowUpMessageSent = true;
            campaignProspect.LastFollowUpMessageSentTimestamp = request.Item.ActualDeliveryDateTimeStamp;
            campaignProspect.SentFollowUpMessageOrderNum = request.Item.MessageOrderNum;

            campaignProspect = await _campaignRepositoryFacade.UpdateCampaignProspectAsync(campaignProspect, ct);
            if (campaignProspect == null)
            {
                _logger.LogError("Failed to update campaign prospect {campaignProspectId} after they have received a follow up message. This means additional follow up messages will be sent", campaignProspectId);
                return false;
            }

            return true;
        }

        public async Task<FollowUpMessagesResponse> GetFollowUpMessagesAsync(string halId, CancellationToken ct = default)
        {
            FollowUpMessagesResponse response = new();
            IList<PublishMessageBody> followUpMessages = await _followUpMessagesMQService.CreateMQFollowUpMessagesAsync(halId, ct);
            if (followUpMessages != null)
            {
                response.Items = followUpMessages.Cast<FollowUpMessageBody>().ToList();
            }

            return response;
        }
    }
}
