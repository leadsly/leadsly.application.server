using Leadsly.Domain.Models.DeepScanProspectsForReplies;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Requests;
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
        public async Task<bool> ProcessCampaignProspectsRepliesAsync(ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing ProcessCampaignProspectsReplies action");
            IList<CampaignProspect> updatedProspects = new List<CampaignProspect>();
            foreach (ProspectRepliedModel prospectReplied in request.Items)
            {
                CampaignProspect prospect = await _campaignRepositoryFacade.GetCampaignProspectByIdAsync(prospectReplied.CampaignProspectId, ct);

                prospect.Replied = true;
                prospect.FollowUpComplete = true;
                prospect.ResponseMessage = prospectReplied.ResponseMessage;
                prospect.RepliedTimestamp = prospectReplied.ResponseMessageTimestamp;
                updatedProspects.Add(prospect);
            }

            updatedProspects = await _campaignRepositoryFacade.UpdateAllCampaignProspectsAsync(updatedProspects, ct);
            bool succeeded = false;
            if (updatedProspects == null)
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
            IList<CampaignProspect> campaignProspects = await GetCampainProspectsAsync(halId);

            IList<NetworkProspectResponse> networkProspects = new List<NetworkProspectResponse>();
            foreach (CampaignProspect campaignProspect in campaignProspects)
            {
                int lastFollowUpMessageOrder = campaignProspect.SentFollowUpMessageOrderNum;
                CampaignProspectFollowUpMessage lastFollowUpMessage = campaignProspect.FollowUpMessages.Where(x => x.Order == lastFollowUpMessageOrder).FirstOrDefault();
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

        private async Task<IList<CampaignProspect>> GetCampainProspectsAsync(string halId)
        {
            IDictionary<string, IList<CampaignProspect>> halsCampaignProspects = new Dictionary<string, IList<CampaignProspect>>();

            IList<CampaignProspect> halCampaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
            IList<CampaignProspect> contactedProspects = halCampaignProspects.Where(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false).ToList();

            return contactedProspects;
        }
    }
}
