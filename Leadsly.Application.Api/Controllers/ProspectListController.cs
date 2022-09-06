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
    public class ProspectListController : ApiControllerBase
    {
        public ProspectListController(ILogger<ProspectListController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ILogger<ProspectListController> _logger;
        private readonly ISupervisor _supervisor;

        [HttpPost("{halId}")]
        public async Task<IActionResult> ProcessProspectListAsync(string halId, CollectedProspectsRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing ProspectList action for HalId {halId}", halId);
            bool succeeded = await _supervisor.ProcessProspectsAsync(request, ct);

            if (succeeded == false)
            {
                _logger.LogDebug("Failed to process prospects for HalId {halId}", halId);
                return BadRequest(ProblemDetailsDescriptions.ProspectListError);
            }

            return Ok();
        }
    }
}
