using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Models.Responses;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<bool> ProcessCampaignProspectsRepliesAsync(Models.Requests.ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing ProcessCampaignProspectsReplies action");
            bool succeeded = false;
            IList<Models.Entities.Campaigns.CampaignProspect> campaignProspectsToUpdate = new List<Models.Entities.Campaigns.CampaignProspect>();
            foreach (Models.Requests.ProspectRepliedRequest prospectReplied in request.Items)
            {
                Models.Entities.Campaigns.CampaignProspect campaignProspectToUpdate = await _campaignRepositoryFacade.GetCampaignProspectByIdAsync(prospectReplied.CampaignProspectId, ct);

                campaignProspectToUpdate.Replied = true;
                campaignProspectToUpdate.FollowUpComplete = true;
                campaignProspectToUpdate.ResponseMessage = prospectReplied.ResponseMessage;
                campaignProspectsToUpdate.Add(campaignProspectToUpdate);
            }

            campaignProspectsToUpdate = await _campaignRepositoryFacade.UpdateAllCampaignProspectsAsync(campaignProspectsToUpdate, ct);
            if (campaignProspectsToUpdate == null)
            {
                _logger.LogDebug("Failed to update campaign prospects");
                succeeded = false;
            }
            else
            {
                _logger.LogDebug("Successfully updated campaign prospects");
                succeeded = true;
            }

            return succeeded;
        }

        public async Task<NetworkProspectsResponse> GetAllNetworkProspectsAsync(string halId, CancellationToken ct = default)
        {
            IList<Leadsly.Domain.Models.Entities.Campaigns.CampaignProspect> campaignProspects = await GetCampainProspectsAsync(halId);

            IList<NetworkProspectResponse> networkProspects = new List<NetworkProspectResponse>();
            foreach (Leadsly.Domain.Models.Entities.Campaigns.CampaignProspect campaignProspect in campaignProspects)
            {
                int lastFollowUpMessageOrder = campaignProspect.SentFollowUpMessageOrderNum;
                Leadsly.Domain.Models.Entities.Campaigns.CampaignProspectFollowUpMessage lastFollowUpMessage = campaignProspect.FollowUpMessages.Where(x => x.Order == lastFollowUpMessageOrder).FirstOrDefault();
                if (lastFollowUpMessage == null)
                {
                    string campaignProspectId = campaignProspect.CampaignProspectId;
                    int orderNum = campaignProspect.SentFollowUpMessageOrderNum;
                    _logger.LogWarning("Could not determine user's last sent follow up message. CampaignProspectId: {campaignProspectId}\r\n FollowUpMessageOrder: {orderNum}", campaignProspectId, orderNum);
                }
                else
                {
                    string prospectName = campaignProspect.Name;
                    _logger.LogDebug("CampaignProspect {prospectName} had a follow up message sent. The follow up message that was sent had the order number of {lastFollowUpMessageOrder}", prospectName, lastFollowUpMessageOrder);
                    NetworkProspectResponse networkProspect = new()
                    {
                        CampaignProspectId = campaignProspect.CampaignProspectId,
                        LastFollowUpMessageContent = lastFollowUpMessage.Content,
                        Name = campaignProspect.Name,
                        ProspectProfileUrl = campaignProspect.ProfileUrl
                    };

                    networkProspects.Add(networkProspect);
                }
            }
            int count = networkProspects.Count();
            _logger.LogDebug("Number of prospects who have received a follow up message is {count}", count);

            return new NetworkProspectsResponse
            {
                Items = networkProspects
            };
        }

        private async Task<IList<Leadsly.Domain.Models.Entities.Campaigns.CampaignProspect>> GetCampainProspectsAsync(string halId)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = new Dictionary<string, IList<CampaignProspect>>();

            IList<Leadsly.Domain.Models.Entities.Campaigns.CampaignProspect> halCampaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
            IList<Leadsly.Domain.Models.Entities.Campaigns.CampaignProspect> contactedProspects = halCampaignProspects.Where(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false).ToList();

            return contactedProspects;
        }
    }
}
