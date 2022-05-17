using Leadsly.Api;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class SendConnectionsController : ApiControllerBase
    {
        public SendConnectionsController(ILogger<SendConnectionsController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<SendConnectionsController> _logger;

        [HttpPost("{halId}")]        
        public async Task<IActionResult> CreateSendConnectionsMessage(string halId, ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing CreateSendConnectionsMessage action for HalId {halId}", halId);

            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessCampaignProspectsRepliedAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to process list of prospects coming from DeepScanProspectsForReplies phase.");
                return BadRequest_CampaignProspectsReplied(result.Failures);
            }

            return Ok();
        }

        [HttpGet("{campaignId}/url")]        
        public async Task<IActionResult> GetSentConnectionsUrlsAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing GetSentConnectionsUrls for CampaignId {campaignId}", campaignId);
            HalOperationResult<IGetSentConnectionsUrlStatusPayload> result = await _supervisor.GetSentConnectionsUrlStatusesAsync<IGetSentConnectionsUrlStatusPayload>(campaignId, ct);
            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to retrieve sent connections url for CampaignId {campaignId}", campaignId);
                return BadRequest_GettingSentConnectionsUrlStatuses(result.Failures);
            }

            return Ok(result.Value);
        }

        [HttpPatch("{campaignId}/url")]        
        public async Task<IActionResult> UpdateSentConnectionsUrlsAsync(string campaignId, [FromBody] UpdateSentConnectionsUrlStatusRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing action UpdateSentConnectionsUrls for CampaignId {campaignId}", campaignId);
            HalOperationResult<IOperationResponse> result = await _supervisor.UpdateSentConnectionsUrlStatusesAsync<IOperationResponse>(campaignId, request, ct);
            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to update sent connection urls for CampaignId {campaignId}", campaignId);
                return BadRequest_UpdatingSentConnectionsUrlStatuses(result.Failures);
            }

            return Ok();
        }

        [HttpPost("{campaignId}/prospects")]        
        public async Task<IActionResult> UpdateConnectionSentProspectsAsync(string campaignId, [FromBody] CampaignProspectListRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing action UpdateConnectionSentProspects for CampaignId {campaignId}", campaignId);
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessConnectionRequestSentForCampaignProspectsAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to update prospects to which connection request has been sent for CampaignId {campaignId}", campaignId);
                return BadRequest_UpdateContactedCampaignProspects(result.Failures);
            }

            return Ok();
        }

    }
}
