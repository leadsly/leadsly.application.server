using Leadsly.Domain.Supervisor;
using Leadsly.Models;
using Leadsly.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{

    /// <summary>
    /// Healthcheck controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class LeadslyController : ApiControllerBase
    {
        public LeadslyController(ISupervisor supervisor, ILogger<LeadslyController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<LeadslyController> _logger;

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SetupUserWithLeadsly(LeadslySetupDTO setupLeasdsly, CancellationToken ct = default)
        {
            _logger.LogTrace("SetupUserWithLeadsly action executed.");

            LeadslySetupResultDTO result = await _supervisor.SetupLeadslyForUserAsync(setupLeasdsly, ct);

            return Ok();
        }

    }
}
