﻿using Leadsly.Domain.Models;
using Leadsly.Domain.Supervisor;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Reports;
using Leadsly.Application.Model.ViewModels.Reports.ApexCharts;
using Leadsly.Application.Model.Entities;
using Leadsly.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using System.Security.Claims;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.ViewModels;
using Microsoft.AspNetCore.JsonPatch;
using Leadsly.Application.Model.Entities.Campaigns;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
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

        [AllowAnonymous]
        [HttpPost]
        [Route("{halId}")]
        public async Task<IActionResult> CreateFollowUpMessagesMessageAsync(string halId, [FromBody] TriggerFollowUpMessageRequest request)
        {
            _logger.LogInformation("Executing CreateFollowUpMessagesMessage action for HalId {halId}", halId);
            await _supervisor.TriggerFollowUpMessagesPhaseAsync(request);
            return Ok();
        }

        [HttpPost("{campaignProspectId}/follow-up")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowUpMessageSentAsync(string campaignProspectId, [FromBody] FollowUpMessageSentRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Exeucting FollowUpMessageSent action for CampaignProspectId {campaignProspectId}", campaignProspectId);
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessFollowUpMessageSentAsync<IOperationResponse>(request, ct);

            //if (result.Succeeded == false)
            //{
            //    return BadRequest_UpdateContactedCampaignProspects(result.Failures);
            //}

            return Ok();
        }

    }
}
