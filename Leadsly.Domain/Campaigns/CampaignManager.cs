using Hangfire;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Domain.Campaigns.Commands;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns
{
    public class CampaignManager : ICampaignManager
    {
        public CampaignManager(ILogger<CampaignManager> logger, ICampaignPhaseProducer campaignPhaseProducer, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _campaignPhaseProducer = campaignPhaseProducer;
        }

        private readonly ICampaignPhaseProducer _campaignPhaseProducer;        
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CampaignManager> _logger;

        public async Task ProcessAllActiveCampaignsAsync()
        {
            //TriggerConstantCampaignPhaseMessages();

            //TriggerConnectionWithdrawPhase();            

            await TriggerRecurringJobsAsync();
        }

        private async Task TriggerRecurringJobsAsync()
        {
            await TriggerMonitorForNewConnectionsPhaseAsync();

            await TriggerProspectListsPhaseAsync();

            await TriggerFollowUpMessagePhaseForUncontactedProspectsAsync();

            await TriggerDeepScanProspectsForRepliesAsync();
        }
        
        private async Task TriggerDeepScanProspectsForRepliesAsync()
        {
            
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                DeepScanProspectsForRepliesCommand command = new DeepScanProspectsForRepliesCommand(messageBrokerOutlet, _serviceProvider);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            // await _campaignPhaseProducer.PublishDeepScanProspectsForRepliesAsync();
        }

        private async Task TriggerFollowUpMessagePhaseForUncontactedProspectsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                UncontactedFollowUpMessageCommand command = new UncontactedFollowUpMessageCommand(messageBrokerOutlet, _serviceProvider);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            //await _campaignPhaseProducer.PublishUncontactedFollowUpMessagePhaseMessagesAsync();
        }

        private async Task TriggerMonitorForNewConnectionsPhaseAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                MonitorForNewConnectionsAllCommand command = new MonitorForNewConnectionsAllCommand(messageBrokerOutlet, _serviceProvider);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }

            // BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishMonitorForNewConnectionsPhaseMessageAsync());
        }

        public async Task TriggerMonitorForNewProspectsPhase(string halId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                MonitorForNewConnectionsCommand command = new MonitorForNewConnectionsCommand(messageBrokerOutlet, _serviceProvider, halId, userId);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }

            //BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishMonitorForNewConnectionsPhaseMessageAsync(halId, userId));
        }

        private async Task TriggerProspectListsPhaseAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                ProspectListsCommand command = new ProspectListsCommand(messageBrokerOutlet, _serviceProvider);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            // BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishProspectListPhaseMessagesAsync());
        }

        private void TriggerConnectionWithdrawPhase()
        {
            BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishConnectionWithdrawPhaseMessages());
        }

        public async Task TriggerProspectListPhaseAsync(string prospectListPhaseId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                ProspectListCommand command = new ProspectListCommand(messageBrokerOutlet, _serviceProvider, prospectListPhaseId, userId);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            // BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishProspectListPhaseMessagesAsync(prospectListPhaseId, userId));
        }

        public async Task TriggerSendConnectionsPhaseAsync(string campaignId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();
                ILogger<SendConnectionsToProspectsCommand> logger = scope.ServiceProvider.GetRequiredService<ILogger<SendConnectionsToProspectsCommand>>();

                SendConnectionsToProspectsCommand command = new SendConnectionsToProspectsCommand(messageBrokerOutlet, _serviceProvider, logger, campaignId, userId);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            // BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishSendConnectionsPhaseMessageAsync(campaignId, userId));
        }

        public async Task TriggerScanProspectsForRepliesPhaseAsync(string halId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                ScanProspectsForRepliesCommand command = new ScanProspectsForRepliesCommand(messageBrokerOutlet, _serviceProvider, halId, userId);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            // BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishScanProspectsForRepliesPhaseAsync(halId, userId));
        }

        public async Task TriggerFollowUpMessagesPhaseAsync(string halId, string userId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                FollowUpMessagesCommand command = new FollowUpMessagesCommand(messageBrokerOutlet, _serviceProvider, halId, userId);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
            // BackgroundJob.Enqueue<ICampaignPhaseProducer>(x => x.PublishFollowUpMessagesPhaseAsync(halId, userId));
        }

        public async Task TriggerFollowUpMessagePhaseAsync(string campaignProspectFollowUpMessageId, string campaignId, DateTimeOffset scheduleTime = default)
        {
            //if(scheduleTime == default)
            //{
            //    await _campaignPhaseProducer.PublishFollowUpMessagePhaseMessageAsync(campaignProspectFollowUpMessageId, campaignId);
            //}
            //else
            //{
            //    BackgroundJob.Schedule<ICampaignPhaseProducer>(x => x.PublishFollowUpMessagePhaseMessageAsync(campaignProspectFollowUpMessageId, campaignId), scheduleTime);
            //}

            using (var scope = _serviceProvider.CreateScope())
            {
                IMessageBrokerOutlet messageBrokerOutlet = scope.ServiceProvider.GetRequiredService<IMessageBrokerOutlet>();

                FollowUpMessageCommand command = new FollowUpMessageCommand(messageBrokerOutlet, _serviceProvider, campaignProspectFollowUpMessageId, campaignId);
                _campaignPhaseProducer.SetCommand(command);
                await _campaignPhaseProducer.ExecuteAsync();
            }
        }
    }
}
