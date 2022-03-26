using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class CampaignService : ICampaignService
    {
        public CampaignService(ICampaignRepository campaignRepository, ILogger<CampaignService> logger)
        {
            _logger = logger;
            _campaignRepository = campaignRepository;
        }

        private ILogger<CampaignService> _logger;
        private ICampaignRepository _campaignRepository;

        public async Task<int> SetDailyLimitAsync(Campaign campaign, CancellationToken ct = default)
        {
            if (campaign.IsWarmUpEnabled == true)
            {
                // this is a new campaign, create a new warm up configuration class configure it and save it to the database
                CampaignWarmUp warmUp = new()
                {
                    DailyLimit = 3,
                    StartDateTimestamp = campaign.StartTimestamp,
                };
                warmUp = await _campaignRepository.CreateCampaignWarmUpAsync(warmUp, ct);
                return warmUp.DailyLimit;
            }
            else
            {
                return campaign.DailyInvites;
            }
        }

        public CampaignProspectList GenerateCampaignProspectList(PrimaryProspectList primaryProspectList, string userId)
        {
            List<CampaignProspect> campaignProspects = new();

            if(primaryProspectList.PrimaryProspects != null)
            {
                foreach (PrimaryProspect primaryProspect in primaryProspectList.PrimaryProspects)
                {
                    campaignProspects.Add(new()
                    {
                        FirstName = primaryProspect.FirstName,
                        Name = primaryProspect.Name,
                        AddedByCampaignId = primaryProspect.AddedByCampaignId,
                        AddedTimestamp = primaryProspect.AddedTimestamp,
                        LastName = primaryProspect.LastName,
                        ProfileUrl = primaryProspect.ProfileUrl,
                        PrimaryProspectListId = primaryProspect.PrimaryProspectListId,

                        ConnectionSent = false,
                        ConnectionSentTimestamp = 0,
                        FollowUpMessageSent = false,
                        LastFollowUpMessageSentTimestamp = 0
                    });
                }
            }            

            List<SearchUrl> searchUrls = new();
            foreach (SearchUrl searchUrl in primaryProspectList.SearchUrls)
            {
                searchUrls.Add(new()
                {
                    Url = searchUrl.Url,
                    PrimaryProspectListId = searchUrl.PrimaryProspectListId                    
                });
            }

            CampaignProspectList newCampaignProspectList = new()
            {
                CampaignProspects = campaignProspects,
                SearchUrls = searchUrls,
                UserId = userId,
                Name = primaryProspectList.Name,
            };

            return newCampaignProspectList;
        }
    }
}
