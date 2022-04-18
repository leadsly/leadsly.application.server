using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.Commands
{
    public class MonitorForNewConnectionsAllCommand : ICommand
    {
        public MonitorForNewConnectionsAllCommand(IMessageBrokerOutlet messageBrokerOutlet, IServiceProvider serviceProvider)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;

        /// <summary>
        /// Triggered once on recurring basis. This phase is triggered once per registered Hal id. This is a passive phase that is supposed to run from the beginning of the work day until the end
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            string queueNameIn = RabbitMQConstants.MonitorNewAcceptedConnections.QueueName;
            string routingKeyIn = RabbitMQConstants.MonitorNewAcceptedConnections.RoutingKey;
            
            IList<MonitorForNewAcceptedConnectionsBody> messageBodies = await CreateMessageBodiesAsync();
            foreach (MonitorForNewAcceptedConnectionsBody body in messageBodies)
            {
                string halId = body.HalId;
                _messageBrokerOutlet.PublishPhase(body, queueNameIn, routingKeyIn, halId);
            }
        }

        private async Task<IList<MonitorForNewAcceptedConnectionsBody>> CreateMessageBodiesAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                IRabbitMQRepository rabbitMQRepository = scope.ServiceProvider.GetRequiredService<IRabbitMQRepository>();
                IRabbitMQProvider rabbitMQProvider = scope.ServiceProvider.GetRequiredService<IRabbitMQProvider>();
                IUserProvider userProvider = scope.ServiceProvider.GetRequiredService<IUserProvider>();

                IList<SocialAccount> socialAccounts = await userProvider.GetAllSocialAccounts();
                IList<SocialAccount> socialAccountsWithActiveCampaigns = socialAccounts.Where(s => s.User.Campaigns.Any(c => c.Active == true)).ToList();

                IList<MonitorForNewAcceptedConnectionsBody> messageBodies = new List<MonitorForNewAcceptedConnectionsBody>();
                // for each hal with active campaigns trigger MonitorForNewProspectsPhase
                foreach (SocialAccount socialAccount in socialAccountsWithActiveCampaigns)
                {
                    MonitorForNewAcceptedConnectionsBody messageBody = await rabbitMQProvider.CreateMonitorForNewAcceptedConnectionsBodyAsync(socialAccount.HalDetails.HalId, socialAccount.UserId, socialAccount.SocialAccountId);

                    messageBodies.Add(messageBody);
                }

                return messageBodies;
            }
        }
    }
}
