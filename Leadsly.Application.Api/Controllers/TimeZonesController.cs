using Leadsly.Domain.Models.ViewModels;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{

    /// <summary>
    /// TimeZones controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class TimeZonesController : ApiControllerBase
    {
        public TimeZonesController(ISupervisor supervisor, ILogger<TimeZonesController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<TimeZonesController> _logger;

        /// <summary>
        /// Gets supported timezones
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetSupportedTimeZonesAsync(CancellationToken ct = default)
        {
            _logger.LogTrace("Get supported time zones action executed.");

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            IList<TimeZoneViewModel> supportedTimeZones = await _supervisor.GetSupportedTimeZonesAsync(ct);
            if (supportedTimeZones == null)
            {
                return BadRequest_GetResource();
            }

            return Ok(supportedTimeZones);
        }
    }
}
