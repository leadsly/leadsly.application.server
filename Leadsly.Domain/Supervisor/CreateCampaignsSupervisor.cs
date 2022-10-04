using Leadsly.Domain.Converters;
using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.Campaigns;
using Microsoft.Extensions.Logging;
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
            newCampaign.FollowUpMessagePhase = CreateFollowUpMessagePhase(newCampaign);
            newCampaign.SentConnectionsStatuses = _createCampaignService.CreateSearchUrlDetails(searchUrls, newCampaign);
            newCampaign.SearchUrlsProgress = _createCampaignService.CreateSearchUrlProgress(searchUrls, newCampaign);
            newCampaign.SendConnectionStages = _createCampaignService.CreateSendConnectionsStages();

            if (request.CampaignDetails.WarmUp == true)
            {
                throw new NotImplementedException();
            }

            newCampaign = await _createCampaignService.CreateAsync(newCampaign, ct);
            if (newCampaign == null)
            {
                return null;
            }

            if (_featureFlagsOptions.AllInOneVirtualAssistant == true)
            {
                HalUnit halUnit = await _halRepository.GetByHalIdAsync(request.HalId, ct);
                string halId = halUnit.HalId;
                DateTimeOffset startDate = _timestampService.ParseDateTimeOffsetLocalized(halUnit.TimeZoneId, halUnit.StartHour);
                DateTimeOffset endDate = _timestampService.ParseDateTimeOffsetLocalized(halUnit.TimeZoneId, halUnit.EndHour);

                _logger.LogDebug("Enqueuing PublishNetworkingPhaseAsync for halId {halId}. This will be enqueued right now", halId);
                _hangfireService.Enqueue<INetworkingJobsService>((x) => x.PublishNetworkingMQMessagesAsync(halId));

                await ScheduleAllInOneVirtualAssistantPhasesAsync(halId, startDate, endDate);
            }
            else
            {
                await _mqCreatorFacade.PublishScanProspectsForRepliesMessageAsync(request.HalId, ct);
                await _mqCreatorFacade.PublishMonitorForNewConnectionsMessageAsync(request.HalId, ct);
                await _mqCreatorFacade.PublishNetworkingMessageAsync(request.HalId, newCampaign, ct);
            }

            CampaignViewModel viewModel = CampaignConverter.Convert(newCampaign);

            return viewModel;
        }

        private async Task ScheduleAllInOneVirtualAssistantPhasesAsync(string halId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            DateTimeOffset now = await _timestampService.GetNowLocalizedAsync(halId);
            if (now > startDate)
            {
                if (now.Minute > 30)
                {
                    startDate = now.AddHours(1).AddMinutes(-now.Minute).AddSeconds(-now.Second);
                }
                else
                {
                    startDate = now.AddMinutes(-now.Minute).AddSeconds(-now.Second);
                }
            }
            endDate = endDate.AddHours(1);
            for (DateTimeOffset date = startDate; date <= endDate; date = date.AddHours(1))
            {
                _hangfireService.Schedule<IAllInOneVirtualAssistantJobService>((x) => x.PublishAllInOneVirtualAssistantPhaseAsync(halId, false, date.ToString()), date);
            }
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

        private FollowUpMessagePhase CreateFollowUpMessagePhase(Campaign campaign)
        {
            FollowUpMessagePhase phase = new()
            {
                Campaign = campaign,
                PhaseType = PhaseType.FollowUpMessage
            };

            return phase;
        }
    }
}
