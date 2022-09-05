﻿using Leadsly.Application.Model;
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
        public async Task<IActionResult> ProcessNewlyDetectedProspectsAsync(string halId, [FromBody] Leadsly.Domain.Models.Requests.RecentlyAddedProspectsRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing NewProspects for HalId {halId} for CheckOffHoursNewConnections", halId);
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessNewlyAcceptedProspectsAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Failed to process new prospects for HalId {halId}", halId);
                return BadRequest_FailedToProcessCampaignPhase();
            }

            _logger.LogInformation("Finished processing new prospects from CheckOffHoursNewConnections", halId);
            return Ok();
        }

    }
}
