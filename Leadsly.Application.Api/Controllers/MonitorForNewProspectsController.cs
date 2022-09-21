using Leadsly.Domain;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MonitorForNewProspectsController : ApiControllerBase
    {
        public MonitorForNewProspectsController(ILogger<MonitorForNewProspectsController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<MonitorForNewProspectsController> _logger;

        [AllowAnonymous]
        [HttpPost("{halId}/new-prospects")]
        public async Task<IActionResult> ProcessNewlyDetectedProspectsAsync(string halId, [FromBody] RecentlyAddedProspectsRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing NewProspects for HalId {halId} for CheckOffHoursNewConnections", halId);
            await _supervisor.ProcessNewlyAcceptedProspectsAsync(halId, request, ct);
            _logger.LogInformation("Finished processing new prospects from CheckOffHoursNewConnections", halId);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("{halId}/previous-prospects")]
        public async Task<IActionResult> GetConnectedNetworkProspectsAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing {0} for HalId {1}", nameof(GetConnectedNetworkProspectsAsync), halId);
            ConnectedNetworkProspectsResponse response = await _supervisor.GetAllPreviouslyConnectedNetworkProspectsAsync(halId, ct);
            if (response == null)
            {
                return BadRequest(ProblemDetailsDescriptions.RecentlyAddedProspects);
            }

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPut("{halId}/previous-prospects")]
        public async Task<IActionResult> UpdatePreviouslyConnectedNetworkProspectsAsync(string halId, UpdateConnectedNetworkProspectsRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing {0} for HalId {1}", nameof(UpdatePreviouslyConnectedNetworkProspectsAsync), halId);
            await _supervisor.UpdatePreviouslyConnectedNetworkProspectsAsync(halId, request, ct);

            return Ok();
        }
    }
}
