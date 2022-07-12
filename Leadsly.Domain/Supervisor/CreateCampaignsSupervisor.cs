using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<CampaignViewModel> CreateCampaignAsync(CreateCampaignRequest request, string userId, CancellationToken ct = default)
        {
            Campaign newCampaign = new()
            {
                Name = request.CampaignDetails.Name,
                StartTimestamp = request.CampaignDetails.StartTimestamp,
                EndTimestamp = request.CampaignDetails.EndTimestamp,
                DailyInvites = request.CampaignDetails.DailyInviteLimit,
                IsWarmUpEnabled = request.CampaignDetails.WarmUp,
                CreatedTimestamp = _timestampService.CreateNowTimestamp(),
                CampaignType = request.CampaignDetails.CampaignType,
                FollowUpMessages = new List<FollowUpMessage>(),
                ApplicationUserId = userId,
                HalId = request.HalId
            };

            IList<string> searchUrls = request.CampaignDetails.PrimaryProspectList.SearchUrls;
            string prospectListName = request.CampaignDetails.PrimaryProspectList.Name;
            PrimaryProspectList primaryProspectList = default;
            if (request.CampaignDetails.PrimaryProspectList.Existing == true)
            {
                primaryProspectList = await _createCampaignService.GetByNameAndUserIdAsync(prospectListName, userId, ct);
            }
            else
            {
                primaryProspectList = await _createCampaignService.CreatePrimaryProspectListAsync(prospectListName, searchUrls, userId, ct);
            }

            newCampaign.CampaignProspectList = new()
            {
                PrimaryProspectList = primaryProspectList,
                SearchUrls = primaryProspectList.SearchUrls.ToList(),
                ProspectListName = primaryProspectList.Name,
            };

            newCampaign.FollowUpMessages = _createCampaignService.CreateFollowUpMessages(newCampaign, request.FollowUpMessages, userId);
            newCampaign.ProspectListPhase = CreateProspectListPhase(searchUrls, newCampaign);
            newCampaign.SendConnectionRequestPhase = CreateSendConnectionRequestPhase(newCampaign);
            newCampaign.FollowUpMessagePhase = CreateFollowUpMessagePhase(newCampaign);
            newCampaign.SentConnectionsStatuses = _createCampaignService.CreateSearchUrlDetails(searchUrls, newCampaign);
            newCampaign.SearchUrlsProgress = _createCampaignService.CreateSearchUrlProgress(searchUrls, newCampaign);
            newCampaign.SendConnectionStages = _createCampaignService.CreateSendConnectionsStages();

            if (request.CampaignDetails.WarmUp == true)
            {
                throw new NotImplementedException();
            }

            newCampaign = await _createCampaignService.CreateAsync(newCampaign, ct);

            await _campaignPhaseClient.HandleNewCampaignMergedAsync(newCampaign);

            CampaignViewModel viewModel = CampaignConverter.Convert(newCampaign);

            return viewModel;
        }

        private ProspectListPhase CreateProspectListPhase(IList<string> searchUrls, Campaign campaign)
        {
            ProspectListPhase phase = new ProspectListPhase
            {
                Campaign = campaign,
                Completed = false,
                PhaseType = PhaseType.ProspectList,
                SearchUrls = searchUrls.ToList()
            };

            return phase;
        }

        private SendConnectionRequestPhase CreateSendConnectionRequestPhase(Campaign campaign)
        {
            SendConnectionRequestPhase phase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.SendConnectionRequests
            };

            return phase;
        }

        private FollowUpMessagePhase CreateFollowUpMessagePhase(Campaign campaign)
        {
            FollowUpMessagePhase phase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.FollwUpMessage
            };

            return phase;
        }
    }
}
