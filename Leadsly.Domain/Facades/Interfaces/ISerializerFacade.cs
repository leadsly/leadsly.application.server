using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using System.Net.Http;
using System.Threading.Tasks;

namespace Leadsly.Domain.Facades.Interfaces
{
    public interface ISerializerFacade
    {
        Task<HalOperationResult<T>> DeserializeConnectAccountResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;

        Task<HalOperationResult<T>> DeserializeEnterTwoFactorAuthCodeResponseAsync<T>(HttpResponseMessage response)
            where T : IOperationResponse;

        byte[] Serialize(ProspectListBody content);
        byte[] Serialize(FollowUpMessageBody content);

        byte[] Serialize(ScanProspectsForRepliesBody content);

        byte[] Serialize(MonitorForNewAcceptedConnectionsBody content);

        byte[] Serialize(SendConnectionsBody content);

        byte[] Serialize(PublishMessageBody content);
    }
}
