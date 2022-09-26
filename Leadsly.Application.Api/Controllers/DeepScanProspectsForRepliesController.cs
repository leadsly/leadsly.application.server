using Leadsly.Domain.Models.Requests;
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
    public class DeepScanProspectsForRepliesController : ApiControllerBase
    {
        public DeepScanProspectsForRepliesController(ILogger<DeepScanProspectsForRepliesController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<DeepScanProspectsForRepliesController> _logger;

        /// <summary>
        /// Handles list of prospects that have replied, after executing the DeepScanProspectsForReplies phase.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{halId}")]
        public async Task<IActionResult> ProcessProspectsRepliedAsync(string halId, ProspectsRepliedRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing DeepScanProspects action for HalId {halId}", halId);

            bool succeeded = await _supervisor.ProcessCampaignProspectsRepliesAsync(request, ct);

            if (succeeded == false)
            {
                _logger.LogDebug("Failed to process list of replies coming from DeepScanProspectsForReplies phase.");
                return BadRequest_CampaignProspectsReplies();
            }

            return Ok();
        }

        //[HttpGet("{halId}/all-network-prospects")]
        //public async Task<IActionResult> GetAllActiveCampaignsNetworkProspects(string halId, CancellationToken ct = default)
        //{
        //    _logger.LogInformation("Executing GetAllActiveCampaignsNetworkProspects for halId {halId}", halId);

        //    NetworkProspectsResponse response = await _supervisor.GetAllNetworkProspectsAsync(halId, ct);

        //    if (response == null)
        //    {
        //        _logger.LogDebug("Failed to retrieve NetworkProspects");
        //        return BadRequest_FailedToGetNetworkProspects();
        //    }

        //    return Ok(response);
        //}

    }
}
