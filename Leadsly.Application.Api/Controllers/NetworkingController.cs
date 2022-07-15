using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class NetworkingController : ApiControllerBase
    {
        public NetworkingController(ILogger<NetworkingController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<NetworkingController> _logger;

        [HttpGet("{campaignId}/url")]
        public async Task<IActionResult> GetSearchUrlsProgress(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing action GetSearchUrlProgress for CampaignId {campaignId}", campaignId);
            HalOperationResult<IOperationResponse> result = await _supervisor.GetSearchUrlProgressAsync<IOperationResponse>(campaignId, ct);
            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to update sent connection urls for CampaignId {campaignId}", campaignId);
                return BadRequest_UpdatingSentConnectionsUrlStatuses(result.Failures);
            }

            return Ok(result.Value);
        }

        [HttpPatch("{searchUrlProgressId}/url")]
        public async Task<IActionResult> UpdateSearchUrlProgress(string searchUrlProgressId, [FromBody] JsonPatchDocument<Domain.Models.Entities.Campaigns.SearchUrlProgress> request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing action UpdateSearchUrlProgress for SearchUrlPrgressId {searchUrlProgressId}", searchUrlProgressId);
            HalOperationResult<IOperationResponse> result = await _supervisor.UpdateSearchUrlProgressAsync<IOperationResponse>(searchUrlProgressId, request, ct);
            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to update search url progress for SearchUrlPrgressId {searchUrlProgressId}", searchUrlProgressId);
                return BadRequest_UpdatingSentConnectionsUrlStatuses(result.Failures);
            }

            return Ok(result.Value);
        }
    }
}
