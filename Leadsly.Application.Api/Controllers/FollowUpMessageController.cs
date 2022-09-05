﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
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
    public class FollowUpMessageController : ApiControllerBase
    {
        public FollowUpMessageController(ILogger<FollowUpMessageController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<FollowUpMessageController> _logger;

        //[HttpPost]
        //[Route("{halId}")]
        //public async Task<IActionResult> CreateFollowUpMessagesMessageAsync(string halId, [FromBody] TriggerFollowUpMessageRequest request)
        //{
        //    _logger.LogInformation("Executing CreateFollowUpMessagesMessage action for HalId {halId}", halId);
        //    await _supervisor.TriggerFollowUpMessagesPhaseAsync(request);
        //    return Ok();
        //}

        [HttpPost("{campaignProspectId}/follow-up")]
        public async Task<IActionResult> ProcessSentFollowUpMessageAsync(string campaignProspectId, [FromBody] SentFollowUpMessageRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Exeucting FollowUpMessageSent action for CampaignProspectId {campaignProspectId}", campaignProspectId);
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessFollowUpMessageSentAsync<IOperationResponse>(request, ct);

            return Ok();
        }

    }
}
