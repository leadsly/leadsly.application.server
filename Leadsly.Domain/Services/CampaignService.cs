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

        public async Task<int> CreateDailyWarmUpLimitConfigurationAsync(long startDateTimestamp, CancellationToken ct = default)
        {            
            // this is a new campaign, create a new warm up configuration class configure it and save it to the database
            CampaignWarmUp warmUp = new()
            {
                DailyLimit = 3,
                StartDateTimestamp = startDateTimestamp,
            };
            warmUp = await _campaignRepository.CreateCampaignWarmUpAsync(warmUp, ct);
            return warmUp.DailyLimit;
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
                        ConnectionSent = false,
                        ConnectionSentTimestamp = 0,
                        FollowUpMessageSent = false,
                        LastFollowUpMessageSentTimestamp = 0,
                        ProfileUrl = primaryProspect.ProfileUrl,
                        Name = primaryProspect.Name
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
                PrimaryProspectList = primaryProspectList,
                SearchUrls = searchUrls,                
                // could be used to allow each campaign to modify its own prospect list name
                ProspectListName = primaryProspectList.Name,
            };

            return newCampaignProspectList;
        }
    }
}
