using Leadsly.Application.Model;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ.Messages;
using Leadsly.Domain.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Decorators
{
    public class SaveBrowserProfileUserProviderDecorator : IUserProvider
    {
        private readonly IMessageBrokerOutlet _messageBrokerOutlet;
        private readonly IUserProvider _userProvider;
        private readonly ILogger<SaveBrowserProfileUserProviderDecorator> _logger;
        private readonly IPersistBrowserProfileMessageFactory _factory;

        public SaveBrowserProfileUserProviderDecorator(
            IUserProvider userProvider,
            IMessageBrokerOutlet messageBrokerOutlet,
            ILogger<SaveBrowserProfileUserProviderDecorator> logger,
            IPersistBrowserProfileMessageFactory factory)
        {
            _messageBrokerOutlet = messageBrokerOutlet;
            _userProvider = userProvider;
            _logger = logger;
            _factory = factory;
        }

        public async Task<SocialAccount> CreateSocialAccountAsync(VirtualAssistant virtualAssistant, string userId, string email, CancellationToken ct = default)
        {
            SocialAccount socialAccount = await _userProvider.CreateSocialAccountAsync(virtualAssistant, userId, email, ct);
            if (socialAccount != null)
            {
                PublishMessageBody mqMessage = _factory.CreateMQMessage();
                _logger.LogDebug("Social account creation succeeded. Sending message to Sidecart to save user's browser profile to S3 bucket");
                string halId = virtualAssistant.HalId;
                string queueNameIn = RabbitMQConstants.PersistBrowserProfile.QueueName;
                string routingKeyIn = RabbitMQConstants.PersistBrowserProfile.RoutingKey;

                _messageBrokerOutlet.PublishPhase(mqMessage, queueNameIn, routingKeyIn, halId, null);
            }

            return socialAccount;
        }

        public async Task<IList<SocialAccount>> GetAllSocialAccounts(CancellationToken ct = default)
        {
            return await _userProvider.GetAllSocialAccounts(ct);
        }

        public async Task<SocialAccount> GetSocialAccountByHalIdAsync(string halId, CancellationToken ct = default)
        {
            return await _userProvider.GetSocialAccountByHalIdAsync(halId, ct);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId, CancellationToken ct = default)
        {
            return await _userProvider.GetUserByIdAsync(userId, ct);
        }
    }
}
