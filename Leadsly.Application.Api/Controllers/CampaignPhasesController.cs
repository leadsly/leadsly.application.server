using Leadsly.Api;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CampaignPhasesController : ApiControllerBase
    {
        public CampaignPhasesController(ILogger<CampaignPhasesController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ILogger<CampaignPhasesController> _logger;
        private readonly ISupervisor _supervisor;

        /// <summary>
        /// This is for processing newly accepted connections on my network page
        /// </summary>
        /// <returns></returns>
        //[HttpPost]
        //[Route("new-my-network-connections")]
        //public async Task<IActionResult> ProcessNewMyNetWorkConnections([FromBody] MyNetworkNewConnectionsRequest newConnectionProspects)
        //{
        //    return Ok();
        //}

        [HttpPost]
        [AllowAnonymous]
        [Route("trigger-send-connection-requests")]
        public IActionResult TriggerSendConnectionRequests([FromBody] TriggerSendConnectionsRequest request)
        {
            _supervisor.TriggerSendConnectionsPhase(request);
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("trigger-scan-prospects-for-replies")]
        public IActionResult TriggerScanProspectsForReplies([FromBody] TriggerScanProspectsForRepliesRequest request)
        {
            _supervisor.TriggerScanProspectsForRepliesPhase(request);
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("trigger-follow-up-messages")]
        public IActionResult TriggerFollowUpMessages([FromBody] TriggerFollowUpMessageRequest request)
        {
            _supervisor.TriggerFollowUpMessagesPhase(request);
            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("process-newly-accepted-prospects")]
        public async Task<IActionResult> ProcessNewlyAcceptedProspectsAsync([FromBody] NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default)
        {
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessNewlyAcceptedProspectsAsync<IOperationResponse>(request, ct);

            if (result.Succeeded == false)
            {
                return BadRequest_FailedToProcessCampaignPhase();
            }

            return Ok();
        }
    }
}
