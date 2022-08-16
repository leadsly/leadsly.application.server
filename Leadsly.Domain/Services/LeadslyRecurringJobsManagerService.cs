using Leadsly.Application.Model;
using Leadsly.Domain.Campaigns.FollowUpMessagesHandler.FollowUpMessages;
using Leadsly.Domain.Campaigns.MonitorForNewConnectionsHandlers;
using Leadsly.Domain.Campaigns.NetworkingHandler;
using Leadsly.Domain.Campaigns.ScanProspectsForRepliesHandlers;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class LeadslyRecurringJobsManagerService : ILeadslyRecurringJobsManagerService
    {
        public LeadslyRecurringJobsManagerService(
            ILogger<LeadslyRecurringJobsManagerService> logger,
            HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler,
            HalWorkCommandHandlerDecorator<RestartResourcesCommand> restartResourcesHandler,
            HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> monitorHandler,
            HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepHandler,
            HalWorkCommandHandlerDecorator<FollowUpMessagesCommand> followUpHandler,
            HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> scanProspectsHandler,
            HalWorkCommandHandlerDecorator<NetworkingCommand> networkingCommandHandler,
            ICampaignRepositoryFacade campaignRepositoryFacade,
            IMemoryCache memoryCache
            )
        {
            _logger = logger;
            _campaignRepositoryFacade = campaignRepositoryFacade;
            _memoryCache = memoryCache;
            _networkingCommandHandler = networkingCommandHandler;
            _offHoursHandler = offHoursHandler;
            _monitorHandler = monitorHandler;
            _deepHandler = deepHandler;
            _followUpHandler = followUpHandler;
            _scanProspectsHandler = scanProspectsHandler;
            _restartResourcesHandler = restartResourcesHandler;
        }

        private readonly ILogger<LeadslyRecurringJobsManagerService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ICampaignRepositoryFacade _campaignRepositoryFacade;
        private readonly HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> _monitorHandler;
        private readonly HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> _offHoursHandler;
        private readonly HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> _deepHandler;
        private readonly HalWorkCommandHandlerDecorator<FollowUpMessagesCommand> _followUpHandler;
        private readonly HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> _scanProspectsHandler;
        private readonly HalWorkCommandHandlerDecorator<NetworkingCommand> _networkingCommandHandler;
        private readonly HalWorkCommandHandlerDecorator<RestartResourcesCommand> _restartResourcesHandler;

        /// <summary>
        /// Restarts hal every morning to ensure fresh state
        /// </summary>
        /// <param name="halId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task RestartHalAsync(string halId)
        {
            //////////////////////////////////////////////////////////////////////////////////////
            ///// Restart Hal
            //////////////////////////////////////////////////////////////////////////////////////
            RestartResourcesCommand restartCommand = new RestartResourcesCommand(halId);
            await _restartResourcesHandler.HandleAsync(restartCommand);
        }

        /// <summary>
        /// Executed each morning at the given StartTime and timezone for each HalUnit.
        /// </summary>
        /// <param name="halId"></param>
        /// <returns></returns>
        public async Task PublishHalPhasesAsync(string halId)
        {
            //////////////////////////////////////////////////////////////////////////////////////
            ///// ScanForNewConnectionsOffHours
            //////////////////////////////////////////////////////////////////////////////////////            
            CheckOffHoursNewConnectionsCommand offHoursCommand = new CheckOffHoursNewConnectionsCommand(halId);
            await _offHoursHandler.HandleAsync(offHoursCommand);

            ////////////////////////////////////////////////////////////////////////////////////
            /// MonitorForNewConnectionsAll
            ////////////////////////////////////////////////////////////////////////////////////            
            MonitorForNewConnectionsCommand monitorForNewConnectionsCommand = new MonitorForNewConnectionsCommand(halId);
            await _monitorHandler.HandleAsync(monitorForNewConnectionsCommand);

            //////////////////////////////////////////////////////////////////////////////////////////////////////
            /// Prospecting [ DeepScanProspectsForReplies OR (FollowUpMessagePhase AND ScanProspectsForReplies) ]
            //////////////////////////////////////////////////////////////////////////////////////////////////////
            await ProspectingPhaseAsync(halId);

            ////////////////////////////////////////////////////////////////////////////////////////////////
            /// NetworkingPhase[ProspectListPhase AND SendConnectionsPhase Merged]
            ////////////////////////////////////////////////////////////////////////////////////////////////
            await NetworkingPhaseAsync(halId);
        }

        public async Task NetworkingPhaseAsync(string halId, CancellationToken ct = default)
        {
            NetworkingCommand networkingCommand = new NetworkingCommand(halId);
            await _networkingCommandHandler.HandleAsync(networkingCommand);
        }

        private async Task ProspectingPhaseAsync(string halId, CancellationToken ct = default)
        {
            if (await PublishDeepScanAsync(halId, ct) == true)
            {
                DeepScanProspectsForRepliesCommand deepScanCommand = new DeepScanProspectsForRepliesCommand(halId);
                await _deepHandler.HandleAsync(deepScanCommand);
            }
            else
            {
                FollowUpMessagesCommand followUpMsgsCommand = new FollowUpMessagesCommand(halId);
                await _followUpHandler.HandleAsync(followUpMsgsCommand);

                ScanProspectsForRepliesCommand scanProspectsCommand = new ScanProspectsForRepliesCommand(halId);
                await _scanProspectsHandler.HandleAsync(scanProspectsCommand);
            }
        }

        /// <summary>
        /// If any of the prospects associated with HalUnit have accepted the connection invite, have gotten a follow up message and have NOT been marked as complete (this happens when all follow up messages go out AND not contact is made within certain time frame)
        /// we then proceed to execute DeepScanProspectsForReplies phase.
        /// </summary>
        /// <param name="halId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<bool> PublishDeepScanAsync(string halId, CancellationToken ct = default)
        {
            IList<CampaignProspect> campaignProspects = await GetAllCampaignProspectsByHalIdAsync(halId, ct);
            if (campaignProspects.Any(p => p.Accepted == true && p.FollowUpMessageSent == true && p.Replied == false && p.FollowUpComplete == false) == true)
            {
                return true;
            }
            return false;
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
