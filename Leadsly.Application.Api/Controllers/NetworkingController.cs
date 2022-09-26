using Leadsly.Domain;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Responses;
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
        public async Task<IActionResult> GetSearchUrlProgressAsync(string campaignId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing action GetSearchUrlProgress for CampaignId {campaignId}", campaignId);
            GetSearchUrlsProgressResponse response = await _supervisor.GetSearchUrlProgressAsync(campaignId, ct);
            if (response == null)
            {
                _logger.LogDebug("Failed to retrieve search urls progress for campaign id {campaignid}", campaignId);
                return BadRequest(ProblemDetailsDescriptions.SearchUrlsProgress);
            }

            return Ok(response);
        }

        [HttpPatch("{searchUrlProgressId}/url")]
        public async Task<IActionResult> UpdateSearchUrlAsync(string searchUrlProgressId, [FromBody] JsonPatchDocument<SearchUrlProgress> request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing action UpdateSearchUrlProgress for SearchUrlPrgressId {searchUrlProgressId}", searchUrlProgressId);
            bool succeeded = await _supervisor.UpdateSearchUrlProgressAsync(searchUrlProgressId, request, ct);
            if (succeeded == false)
            {
                _logger.LogDebug("Failed to update search url progress for SearchUrlPrgressId {searchUrlProgressId}", searchUrlProgressId);
                return BadRequest(ProblemDetailsDescriptions.UpdatingSentConnectionsUrlStatuses);
            }

            return Ok();
        }

        //[HttpGet("{halId}/messages")]
        //public async Task<IActionResult> GetMessagesAsync(string halId, CancellationToken ct = default)
        //{

        //}
    }
}
