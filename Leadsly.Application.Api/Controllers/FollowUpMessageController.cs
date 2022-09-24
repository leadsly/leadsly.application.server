using Leadsly.Domain;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.Responses;
using Leadsly.Domain.MQ.Messages;
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

        [HttpPost("{campaignProspectId}/follow-up")]
        public async Task<IActionResult> ProcessSentFollowUpMessageAsync(string campaignProspectId, [FromBody] SentFollowUpMessageRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Exeucting FollowUpMessageSent action for CampaignProspectId {campaignProspectId}", campaignProspectId);
            bool succeeded = await _supervisor.ProcessSentFollowUpMessageAsync(campaignProspectId, request, ct);
            if (succeeded == false)
            {
                return BadRequest(ProblemDetailsDescriptions.ProcessSentFollowUpMessage);
            }

            return Ok();
        }

        [HttpGet("{halId}/messages")]
        public async Task<IActionResult> GetMessagesAsync(string halId, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing {0}, to get all {1} messages for {2}", nameof(GetMessagesAsync), nameof(FollowUpMessageBody), halId);

            FollowUpMessagesResponse response = await _supervisor.GetFollowUpMessagesAsync(halId, ct);
            if (response == null)
            {
                return BadRequest(ProblemDetailsDescriptions.GetFollowUpMessages);
            }

            return Ok(response);
        }
    }
}
