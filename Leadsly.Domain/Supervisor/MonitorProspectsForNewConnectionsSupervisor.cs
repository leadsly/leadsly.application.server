using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.MonitorForNewConnections;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public const string GetSocialAccountByHalIdCacheKey = "social-account-by-hal-id";
        public async Task<ConnectedNetworkProspectsResponse> GetAllPreviouslyConnectedNetworkProspectsAsync(string halId, CancellationToken ct = default)
        {
            SocialAccount socialAccount = await GetSocialAccountAsync($"{GetSocialAccountByHalIdCacheKey}-{halId}", halId, ct);

            if (socialAccount == null)
            {
                _logger.LogError("No SocialAccount has been found that is associated with HalId {0}", halId);
                return null;
            }

            ConnectedNetworkProspectsResponse response = new()
            {
                TotalConnectionsCount = socialAccount.TotalConnectionsCount
            };

            IList<RecentlyAddedProspect> recentlyAddedProspects = await _recentlyAddedRepository.GetAllBySocialAccountIdAsync(socialAccount.SocialAccountId, ct);

            if (recentlyAddedProspects == null)
            {
                response.Items = new List<RecentlyAddedProspectModel>();
            }
            else
            {
                response.Items = RecentlyAddedProspectConvert.ConvertList(socialAccount.RecentlyAddedProspects);
            }

            return response;
        }

        public async Task UpdatePreviouslyConnectedNetworkProspectsAsync(string halId, UpdateConnectedNetworkProspectsRequest request, CancellationToken ct)
        {
            SocialAccount socialAccount = await GetSocialAccountAsync($"{GetSocialAccountByHalIdCacheKey}-{halId}", halId, ct);

            if (socialAccount == null)
            {
                _logger.LogError("No SocialAccount has been found that is associated with HalId {0}", halId);
                return;
            }

            if (await _recentlyAddedRepository.DeleteAllBySocialAccountIdAsync(socialAccount.SocialAccountId, ct) == false)
            {
                _logger.LogError("Failed to delete {0} for HalId {1} and SocialAccountId {2}", nameof(RecentlyAddedProspect), halId, socialAccount.SocialAccountId);
            }

            IList<RecentlyAddedProspect> updated = RecentlyAddedProspectConvert.ConvertList(request.Items, socialAccount.SocialAccountId);

            updated = await _recentlyAddedRepository.CreateAllAsync(updated, ct);

            if (updated == null)
            {
                _logger.LogError("Failed to create new {0} for HalId {1} and SocialAccountId {2}", nameof(RecentlyAddedProspect), halId, socialAccount.SocialAccountId);
            }

            socialAccount.TotalConnectionsCount = request.TotalConnectionsCount;
            socialAccount = await _userProvider.UpdateSocialAccountAsync(socialAccount, ct);
            if (socialAccount == null)
            {
                // handle error
            }
        }

        private async Task<SocialAccount> GetSocialAccountAsync(string cacheKey, string halId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(cacheKey, out SocialAccount socialAccount) == false)
            {
                socialAccount = await _userProvider.GetSocialAccountByHalIdAsync(halId, ct);
                _memoryCache.Set(cacheKey, socialAccount, TimeSpan.FromMinutes(15));
            }

            return socialAccount;
        }

        public async Task ProcessNewlyAcceptedProspectsAsync(string halId, RecentlyAddedProspectsRequest request, CancellationToken ct = default)
        {
            bool anyCampaignProspects = false;
            IList<Campaign> activeCampaigns = new List<Campaign>();
            foreach (RecentlyAddedProspectModel recentlyAddedProspect in request.Items)
            {
                string searchProfileUrl = recentlyAddedProspect.ProfileUrl.TrimEnd('/');

                CampaignProspect prospect = await _campaignRepositoryFacade.GetCampaignProspectByProfileUrlAsync(searchProfileUrl, halId, request.ApplicationUserId, ct);
                if (prospect != null)
                {
                    anyCampaignProspects = true;
                    prospect.Accepted = true;
                    prospect.AcceptedTimestamp = recentlyAddedProspect.AcceptedRequestTimestamp;

                    if (prospect.Campaign != null)
                    {
                        _logger.LogDebug("Campaign prospect {prospectId} has a campaign associated with them", prospect.CampaignProspectId);

                        prospect = await _campaignRepositoryFacade.UpdateCampaignProspectAsync(prospect, ct);
                        if (prospect == null)
                        {
                            _logger.LogError("Failed to update CampaignProspect {campaignProspect}. Updating was responsible for updating Accepted property to true", prospect.CampaignProspectId);
                        }
                        else
                        {
                            activeCampaigns.Add(prospect.Campaign);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Campaign prospect {prospectId} does not have a campaign associated with them", prospect.CampaignProspectId);
                    }
                }
            }

            if (anyCampaignProspects == true)
            {
                _logger.LogInformation("Publishing FollowUpMessages for halId {halId}", halId);
                await _mqCreatorFacade.PublishFollowUpMessagesMessageAsync(halId, activeCampaigns, ct);
            }
        }
    }
}
