using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class CreateCampaignService : ICreateCampaignService
    {
        public CreateCampaignService(ICampaignRepositoryFacade campaignRepositoryFacde, ITimestampService timestampService, ILogger<CreateCampaignService> logger)
        {
            _campaignRepositoryFacade = campaignRepositoryFacde;
            _logger = logger;
            _timestampService = timestampService;
        }

        private readonly ITimestampService _timestampService;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly ILogger<CreateCampaignService> _logger;
        public async Task<Campaign> CreateAsync(Campaign newCampaign, CancellationToken ct = default)
        {
            newCampaign = await _campaignRepositoryFacade.CreateCampaignAsync(newCampaign, ct);
            if (newCampaign == null)
            {
                _logger.LogError("Campaign creation failed");
            }

            return newCampaign;
        }

        public IList<FollowUpMessage> CreateFollowUpMessages(Campaign newCampaign, List<FollowUpMessageViewModel> followUpMessages, string userId)
        {
            IList<FollowUpMessage> followUpMsgs = new List<FollowUpMessage>();
            foreach (FollowUpMessageViewModel followUpMsg in followUpMessages)
            {
                FollowUpMessage followUpMessage = new()
                {
                    Campaign = newCampaign,
                    Content = followUpMsg.Content,
                    Order = followUpMsg.Order,
                    Delay = new()
                    {
                        Unit = followUpMsg.Delay.Unit,
                        Value = followUpMsg.Delay.Value
                    }
                };
                followUpMsgs.Add(followUpMessage);
            }

            return followUpMsgs;
        }

        public async Task<PrimaryProspectList> CreatePrimaryProspectListAsync(string prospectListName, IList<string> searchUrls, string userId, CancellationToken ct = default)
        {
            PrimaryProspectList primaryProspectList = new()
            {
                Name = prospectListName,
                SearchUrls = new List<SearchUrl>(),
                UserId = userId,
                CreatedTimestamp = _timestampService.CreateNowTimestamp()
            };

            foreach (string searchUrl in searchUrls)
            {
                primaryProspectList.SearchUrls.Add(new()
                {
                    PrimaryProspectList = primaryProspectList,
                    Url = searchUrl
                });
            }

            primaryProspectList = await _campaignRepositoryFacade.CreatePrimaryProspectListAsync(primaryProspectList, ct);
            if (primaryProspectList == null)
            {
                _logger.LogError("Failed to create primary prospect list");
            }

            return primaryProspectList;
        }

        public IList<SearchUrlDetails> CreateSearchUrlDetails(IList<string> searchUrls, Campaign campaign)
        {
            IList<SearchUrlDetails> sentConnectionsSearchUrlStatuses = new List<SearchUrlDetails>();
            foreach (string searchUrl in searchUrls)
            {
                SearchUrlDetails sentConnectionsSearchUrlStatus = new()
                {
                    Campaign = campaign,
                    CurrentUrl = searchUrl,
                    FinishedCrawling = false,
                    StartedCrawling = false,
                    OriginalUrl = searchUrl,
                    WindowHandleId = string.Empty,
                    LastActivityTimestamp = 0
                };
                sentConnectionsSearchUrlStatuses.Add(sentConnectionsSearchUrlStatus);
            }

            return sentConnectionsSearchUrlStatuses;
        }

        public IList<SearchUrlProgress> CreateSearchUrlProgress(IList<string> searchUrls, Campaign campaign)
        {
            IList<SearchUrlProgress> searchUrlsProgress = new List<SearchUrlProgress>();

            foreach (string searchUrl in searchUrls)
            {
                SearchUrlProgress searchUrlProgress = new()
                {
                    Campaign = campaign,
                    LastPage = 1,
                    LastProcessedProspect = 0,
                    TotalSearchResults = 0,
                    SearchUrl = searchUrl,
                    WindowHandleId = string.Empty
                };
                searchUrlsProgress.Add(searchUrlProgress);
            }

            return searchUrlsProgress;
        }

        public async Task<PrimaryProspectList> GetByNameAndUserIdAsync(string prospectListName, string userId, CancellationToken ct = default)
        {
            PrimaryProspectList primaryProspectList = await _campaignRepositoryFacade.GetPrimaryProspectListByNameAndUserIdAsync(prospectListName, userId, ct);
            if (primaryProspectList == null)
            {
                _logger.LogError($"PrimaryProspectList with name {prospectListName} and userId {userId} not found");
            }

            return primaryProspectList;
        }

        public IList<SendConnectionsStage> CreateSendConnectionsStages()
        {
            IList<SendConnectionsStage> connectionsStages = new List<SendConnectionsStage>();
            for (int i = 0; i < 3; i++)
            {
                SendConnectionsStage sendConnectionsStage = new();
                if (i == 0)
                {
                    sendConnectionsStage = new SendConnectionsStage
                    {
                        StartTime = "8:00 AM",
                        Order = i + 1
                    };
                }
                else if (i == 1)
                {
                    sendConnectionsStage = new SendConnectionsStage
                    {
                        StartTime = "12:00 PM",
                        Order = i + 1
                    };
                }
                else
                {
                    sendConnectionsStage = new SendConnectionsStage
                    {
                        StartTime = "5:00PM",
                        Order = i + 1
                    };
                }

                connectionsStages.Add(sendConnectionsStage);
            }

            return connectionsStages;
        }
    }
}
