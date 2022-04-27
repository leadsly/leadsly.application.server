using Leadsly.Api;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CampaignProspectsController : ApiControllerBase
    {
        public CampaignProspectsController(ISupervisor supervisor)
        {
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;

        [HttpPost("{id}/follow-up")]
        [AllowAnonymous]
        public async Task<IActionResult> FollowUpMessageSent(string id, [FromBody] FollowUpMessageSentRequest request, CancellationToken ct = default)
        {
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessFollowUpMessageSentAsync<IOperationResponse>(request, ct);

            //if (result.Succeeded == false)
            //{
            //    return BadRequest_UpdateContactedCampaignProspects(result.Failures);
            //}

            return Ok();
        }
    }
}
