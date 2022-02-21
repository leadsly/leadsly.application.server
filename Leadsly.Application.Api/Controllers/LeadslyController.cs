using Leadsly.Domain.Supervisor;
using Leadsly.Models;
using Leadsly.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Domain.ViewModels;
using Leadsly.Domain.ViewModels.Cloud;

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

        [HttpPost("setup")]
        [AllowAnonymous]
        public async Task<IActionResult> SetupAccountWithLeadsly(SetupLeadslyViewModel setupLeasdsly, CancellationToken ct = default)
        {
            _logger.LogTrace("SetupUserWithLeadsly action executed.");

            LeadslySetupResultViewModel result = await _supervisor.SetupLeadslyForUserAsync(setupLeasdsly, ct);

            if(result.Succeeded == false)
            {
                _logger.LogTrace("[SetupLeadslyForUserAsync] did not succeeded.");
                if (result.Failures.Count > 0)
                {
                    return BadRequest_LeadslySetup(result.Failures);
                }
                else
                {
                    return BadRequest_LeadslySetup();
                }
            }

            return Ok(result);
        }

    }
}
