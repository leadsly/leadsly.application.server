using Leadsly.Domain;
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

    }
}
