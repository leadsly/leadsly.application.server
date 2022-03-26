using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<HalOperationResultViewModel<T>> CreateCampaignAsync<T>(CreateCampaignRequest request, string userId, CancellationToken ct = default)
            where T : IOperationResponseViewModel
        {
            HalOperationResultViewModel<T> result = new();

            Campaign newCampaign = new()
            {
                Name = request.CampaignDetails.Name,
                StartTimestamp = request.CampaignDetails.StartTimestamp,
                EndTimestamp = request.CampaignDetails.EndTimestamp,
                DailyInvites = request.CampaignDetails.DailyInviteLimit,
                IsWarmUpEnabled = request.CampaignDetails.WarmUp,
                CampaignType = request.CampaignDetails.CampaignType,
                FollowUpMessages = new List<FollowUpMessage>(),
                ApplicationUserId = userId
            };

            CampaignProspectList campaignProspectList = await CreateCampaignProspectListAsync(request, userId, ct);
            newCampaign.CampaignProspectList = campaignProspectList;

            foreach (FollowUpMessageViewModel followUpMsg in request.FollowUpMessages)
            {
                FollowUpMessage followUpMessage = new()
                {
                    Campaign = newCampaign,
                    Content = followUpMsg.Content,
                    Order = followUpMsg.Order,
                    DateTimeDelayTimestamp = followUpMsg.DateTimeDelayTimestamp
                };

                newCampaign.FollowUpMessages.Add(followUpMessage);
            }

            // get hal id for the connected account request.ConnectedAccount
            HalUnit halDetails = await _leadslyHalProvider.GetHalDetailsByConnectedAccountUsernameAsync(request.ConnectedAccount, ct);
            newCampaign.HalId = halDetails.HalId;

            Campaign newCampaignWithPhases = CreateCampaignPhases(newCampaign, request.CampaignDetails.PrimaryProspectList.Existing, ct);

            SearchUrl firstSearchUrl = newCampaign.CampaignProspectList.SearchUrls.FirstOrDefault();
            SearchUrl nextSearchUrl = newCampaign.CampaignProspectList.SearchUrls.Skip(1).Take(1).FirstOrDefault();
            SentConnectionsStatus sentConnectionsStatus = new()
            {
                Campaign = newCampaignWithPhases,
                LastVisistedPageUrl = firstSearchUrl,
                NextPageUrl = nextSearchUrl,
                LastProspectHitListPosition = 0
            };
            newCampaignWithPhases.SentConnectionsStatus = sentConnectionsStatus;

            newCampaignWithPhases = await _campaignRepository.CreateAsync(newCampaignWithPhases, ct);
            if (newCampaignWithPhases == null)
            {
                result.OperationResults.Failures.Add(new()
                {
                    Code = Codes.DATABASE_OPERATION_ERROR,
                    Reason = "Failed to create new campaign",
                    Detail = "Error occured while saving the new campaign and it's phases"
                });
                return result;
            }

            _campaignProvider.ProcessNewCampaign(newCampaign);

            result.OperationResults.Succeeded = true;
            return result;
        }

        private async Task<CampaignProspectList> CreateCampaignProspectListAsync(CreateCampaignRequest request, string userId,  CancellationToken ct = default)
        {
            PrimaryProspectList primaryProspectList = default;
            if (request.CampaignDetails.PrimaryProspectList.Existing == true)
            {
                // grab existing prospect list by prospect list name and user id
                primaryProspectList = await _prospectListRepository.GetPrimaryProspectListByNameAndUserIdAsync(request.CampaignDetails.PrimaryProspectList.Name, userId, ct);
            }
            else
            {
                primaryProspectList = new()
                {
                    Name = request.CampaignDetails.PrimaryProspectList.Name,
                    SearchUrls = new List<SearchUrl>(),
                    UserId = userId
                };

                foreach (string searchUrl in request.CampaignDetails.PrimaryProspectList.SearchUrls)
                {
                    primaryProspectList.SearchUrls.Add(new()
                    {
                        PrimaryProspectList = primaryProspectList,
                        Url = searchUrl
                    });
                }

                primaryProspectList = await _prospectListRepository.CreatePrimaryProspectListAsync(primaryProspectList, ct);
            }

            return _campaignProvider.CreateCampaignProspectList(primaryProspectList, userId);
        }

        private Campaign CreateCampaignPhases(Campaign newCampaign, bool useExistingProspectList, CancellationToken ct = default)            
        {
            CampaignType campaignType = default;
            switch (newCampaign.CampaignType)
            {
                case CampaignTypeEnum.None:                    
                    break;
                case CampaignTypeEnum.Invitations:
                    campaignType = new InvitationsCampaign();
                    break;
                case CampaignTypeEnum.FollowUp:
                    campaignType = new FollowUpCampaign();
                    break;
                case CampaignTypeEnum.ProfileVisits:
                    break;
                default:
                    break;
            }

            Campaign campaign = campaignType.GeneratePhases(newCampaign, useExistingProspectList);

            return campaign;
        }
    }
}
