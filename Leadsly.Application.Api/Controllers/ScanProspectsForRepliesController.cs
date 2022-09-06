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
    public class ScanProspectsForRepliesController : ApiControllerBase
    {
        public ScanProspectsForRepliesController(ILogger<ScanProspectsForRepliesController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<ScanProspectsForRepliesController> _logger;

        [HttpPost("{halId}/prospects-replied")]
        public async Task<IActionResult> ProcessNewMessagesAsync(string halId, NewMessagesRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("Executing ProspectsReplied action for HalId {halId}", halId);

            bool succeeded = await _supervisor.ProcessPotentialProspectsRepliesAsync(halId, request, ct);

            if (succeeded == false)
            {
                _logger.LogDebug("Failed to process prospects that have replied for HalId {halId}", halId);
                return BadRequest_ProcessPotentialProspectsReplies();
            }

            return Ok();
        }

    }
}
