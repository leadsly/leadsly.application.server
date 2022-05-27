using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessages;
using Leadsly.Domain.Campaigns.NetworkingHandler;
using Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectLists;
using Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers;
using Leadsly.Domain.Campaigns.SendConnectionsToProspectsHandlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public class PhaseManager : IPhaseManager
    {
        public PhaseManager(
            ILogger<PhaseManager> logger,            
            HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepHandler,
            HalWorkCommandHandlerDecorator<ProspectListsCommand> prospectListsHandler,
            HalWorkCommandHandlerDecorator<FollowUpMessagesCommand> followUpHandler,
            HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> scanProspectsHandler,
            HalWorkCommandHandlerDecorator<SendConnectionsToProspectsCommand> sendConnectionsHandler,
            HalWorkCommandHandlerDecorator<NetworkingCommand> networkingCommandHandler,
            IMemoryCache memoryCache,
            IHalRepository halRepository,
            ISocialAccountRepository socialAccountRepository,
            ICampaignRepositoryFacade campaignRepositoryFacade
            )
        {
            _socialAccountRepository = socialAccountRepository;
            _logger = logger;
            _networkingCommandHandler = networkingCommandHandler;
            _deepHandler = deepHandler;
            _followUpHandler = followUpHandler;
            _prospectListsHandler = prospectListsHandler;
            _scanProspectsHandler = scanProspectsHandler;
            _memoryCache = memoryCache;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _sendConnectionsHandler = sendConnectionsHandler;
            _halRepository = halRepository;
        }

        private readonly ISocialAccountRepository _socialAccountRepository;
        private readonly ILogger<PhaseManager> _logger;
        private readonly IHalRepository _halRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> _deepHandler;
        private HalWorkCommandHandlerDecorator<FollowUpMessagesCommand> _followUpHandler;
        private HalWorkCommandHandlerDecorator<ProspectListsCommand> _prospectListsHandler;
        private HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> _scanProspectsHandler;
        private HalWorkCommandHandlerDecorator<SendConnectionsToProspectsCommand> _sendConnectionsHandler;
        private HalWorkCommandHandlerDecorator<NetworkingCommand> _networkingCommandHandler;

        #region NetworkingPhase

        public async Task NetworkingPhaseAsync(CancellationToken ct = default)
        {
            IList<SocialAccount> allSocialAccounts = await GetAllSocialAccountsAsync(ct);
            IList<SocialAccount> allSocialAccountsNetworking = allSocialAccounts.Where(s => s.RunProspectListFirst == false).ToList();
            IList<string> networkingHalIds = allSocialAccountsNetworking.Select(s => s.HalDetails.HalId).ToList();

            NetworkingCommand networkingCommand = new NetworkingCommand(networkingHalIds);
            await _networkingCommandHandler.HandleAsync(networkingCommand);
        }

        #endregion

        #region NetworkingConnectionsPhase
        public async Task NetworkingConnectionsPhaseAsync(CancellationToken ct = default)
        {
            IList<SocialAccount> allSocialAccounts = await GetAllSocialAccountsAsync();
            IList<SocialAccount> allSocialAccountsProspectListFirst = allSocialAccounts.Where(s => s.RunProspectListFirst == true).ToList();

            IList<string> halIdsProspectListPhaseFirst = allSocialAccountsProspectListFirst.Select(s => s.HalDetails.HalId).ToList();
            if(halIdsProspectListPhaseFirst.Count > 0)
            {
                IList<string> incompleteHalIds = await GetHalIdsWithIncompleteProspectListPhases(halIdsProspectListPhaseFirst, ct);

                if (incompleteHalIds.Count > 0)
                {
                    ProspectListsCommand prospectListsCommand = new ProspectListsCommand(incompleteHalIds);
                    await _prospectListsHandler.HandleAsync(prospectListsCommand);
                }

                IList<string> halIds = await GetAllHalIdsWithActiveCampaignsAsync(ct);
                if (incompleteHalIds.Count > 0)
                {
                    IList<string> sendConnectionsHalIds = halIds.Where(id => incompleteHalIds.Any(incId => incId != id)).ToList();
                    if (sendConnectionsHalIds.Count > 0)
                    {
                        SendConnectionsToProspectsCommand sendConnectionsProspectsCommand = new SendConnectionsToProspectsCommand(sendConnectionsHalIds);
                        await _sendConnectionsHandler.HandleAsync(sendConnectionsProspectsCommand);
                    }
                }
                else
                {
                    // if we do not have any incomplete ProspectLists then fire off send connections to prospects phase
                    SendConnectionsToProspectsCommand sendConnectionsProspectsCommand = new SendConnectionsToProspectsCommand(halIds);
                    await _sendConnectionsHandler.HandleAsync(sendConnectionsProspectsCommand);
                }
            }            
        }

        private async Task<IList<string>> GetHalIdsForSendConnectionsPhaseAsync(CancellationToken ct = default)
        {
            IList<string> halIdsCompleteProspectListPhases = new List<string>();

            IList<string> halIds = await GetAllHalIdsAsync(ct);
            foreach (string halId in halIds)
            {
                if (await _campaignRepositoryFacade.AnyIncompleteProspectListPhasesByHalIdAsync(halId) == false)
                {
                    halIdsCompleteProspectListPhases.Add(halId);
                }
            }

            return halIdsCompleteProspectListPhases;

        }

        private async Task<IList<string>> GetHalIdsWithIncompleteProspectListPhases(IList<string> hallIdsProspectListPhaseFirst, CancellationToken ct = default)
        {
            IList<string> halIdsIncompleteProspectListPhases = new List<string>();
            foreach (string halId in hallIdsProspectListPhaseFirst)
            {
                if (await _campaignRepositoryFacade.AnyIncompleteProspectListPhasesByHalIdAsync(halId) == true)
                {
                    halIdsIncompleteProspectListPhases.Add(halId);
                }
            }

            return halIdsIncompleteProspectListPhases;
        }

        private async Task<IList<string>> GetHalIdsForIncompleteProspectListPhasesAsync(CancellationToken ct = default)
        {
            IList<string> halIdsIncompleteProspectListPhases = new List<string>();

            IList<string> halIds = await GetAllHalIdsAsync(ct);
            foreach (string halId in halIds)
            {
                if (await _campaignRepositoryFacade.AnyIncompleteProspectListPhasesByHalIdAsync(halId) == true)
                {
                    halIdsIncompleteProspectListPhases.Add(halId);
                }
            }

            return halIdsIncompleteProspectListPhases;
        }

        private async Task<IList<SocialAccount>> GetAllSocialAccountsAsync(CancellationToken ct = default)
        {
            if(_memoryCache.TryGetValue(CacheKeys.AllSocialAccounts, out IList<SocialAccount> socialAccounts) == false)
            {
                socialAccounts = await _socialAccountRepository.GetAllAsync(ct);
                if(socialAccounts.Count > 0)
                {
                    _memoryCache.Set(CacheKeys.AllSocialAccounts, socialAccounts, TimeSpan.FromMinutes(5));
                }
            }            

            return socialAccounts;
        }

        #endregion

        #region ProspectingPhase
        public async Task ProspectingPhaseAsync(CancellationToken ct = default)
        {
            // determine if DeepScanProspectsForReplies phase should go out or if
            // follow up message phase and scan prospects for replies message should be triggered instead
            IList<string> deepScanHalIds = await GetHalIdsForDeepScanPhaseAsync(ct);
            if (deepScanHalIds.Count > 0)
            {
                DeepScanProspectsForRepliesCommand deepScanCommand = new DeepScanProspectsForRepliesCommand(deepScanHalIds);
                await _deepHandler.HandleAsync(deepScanCommand);
            }

            IList<string> halIds = await GetAllHalIdsWithActiveCampaignsAsync(ct);
            IList<string> directHalIds = new List<string>();
            if (deepScanHalIds.Count > 0)
            {
                directHalIds = halIds.Where(id => deepScanHalIds.Any(deepHalId => deepHalId != id)).ToList();
            }
            else
            {
                directHalIds = halIds;
            }            

            // trigger follow up message phase and then ScanProspectsForRepliesPhase            
            if(directHalIds.Count > 0)
            {
                FollowUpMessagesCommand followUpMsgsCommand = new FollowUpMessagesCommand(directHalIds);
                await _followUpHandler.HandleAsync(followUpMsgsCommand);

                ScanProspectsForRepliesCommand scanProspectsCommand = new ScanProspectsForRepliesCommand(directHalIds);
                await _scanProspectsHandler.HandleAsync(scanProspectsCommand);
            }                     
        }

        private async Task<IList<string>> GetHalIdsForDeepScanPhaseAsync(CancellationToken ct = default)
        {
            IList<string> deepScanHalIds = new List<string>();

            IList<string> halIds = await GetAllHalIdsAsync(ct);
            foreach (string halId in halIds)
            {
                IList<CampaignProspect> campaignProspects = await GetAllCampaignProspectsByHalIdAsync(halId, ct);
                if (campaignProspects.Any(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false) == true)
                {
                    deepScanHalIds.Add(halId);
                }
            }

            return deepScanHalIds;
        }

        #endregion

        private async Task<IList<string>> GetAllHalIdsAsync(CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(CacheKeys.AllHalIds, out IList<string> halIds) == false)
            {
                halIds = await _halRepository.GetAllHalIdsAsync(ct);
                if (halIds.Count > 0)
                {
                    _memoryCache.Set(CacheKeys.AllHalIds, halIds, TimeSpan.FromMinutes(5));
                }
            }

            return halIds;
        }

        private async Task<IList<string>> GetAllHalIdsWithActiveCampaignsAsync(CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(CacheKeys.AllActiveCampaigns, out IList<Campaign> activeCampaigns) == false)
            {
                activeCampaigns = await _campaignRepositoryFacade.GetAllActiveCampaignsAsync(ct);
                if(activeCampaigns.Count > 0)
                {
                    _memoryCache.Set(CacheKeys.AllActiveCampaigns, activeCampaigns);
                }
            }

            return activeCampaigns.Select(c => c.HalId).Distinct().ToList();
        }

        private async Task<IList<CampaignProspect>> GetAllCampaignProspectsByHalIdAsync(string halId, CancellationToken ct = default)
        {
            if (_memoryCache.TryGetValue(CacheKeys.AllHalIds, out IList<CampaignProspect> campaignProspects) == false)
            {
                campaignProspects = await _campaignRepositoryFacade.GetAllActiveCampaignProspectsByHalIdAsync(halId);
                if (campaignProspects.Count > 0)
                {
                    _memoryCache.Set(halId, campaignProspects, TimeSpan.FromMinutes(5));
                }
            }

            return campaignProspects;
        }        
    }
}
