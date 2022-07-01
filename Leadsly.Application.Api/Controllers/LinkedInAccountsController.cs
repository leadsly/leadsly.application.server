using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{

    /// <summary>
    /// Leadsly controller.
    /// </summary>
    [ApiController]
    [Route("linkedin-accounts/{userId}")]
    public class LinkedInAccountsController : ApiControllerBase
    {
        public LinkedInAccountsController(ISupervisor supervisor, ILogger<LeadslyController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<LeadslyController> _logger;

        /// <summary>
        /// Whether user is connected to LinkedIn.
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetConnectedAccountAsync(string userId, CancellationToken ct = default)
        {
            _logger.LogTrace("Connected action executed.");

            ConnectedViewModel connected = await _supervisor.GetConnectedAccountAsync(userId, ct);

            return Ok(connected);
        }

        // "connect"

        // "disconnect" 

    }
}
