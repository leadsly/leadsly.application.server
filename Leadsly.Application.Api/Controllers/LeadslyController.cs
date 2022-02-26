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
using Leadsly.Domain.ViewModels.LeadslyBot;
using System.Net.Http;

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
        public async Task<IActionResult> SetupAccountWithLeadsly(SetupAccountViewModel setupLeasdsly, CancellationToken ct = default)
        {
            _logger.LogTrace("SetupUserWithLeadsly action executed.");

            SetupAccountResultViewModel result = await _supervisor.LeadslyAccountSetupAsync(setupLeasdsly, ct);

            if(result.Succeeded == false && result.RequiresNewCloudResource == false)
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

        [HttpPost("webdriver")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestNewDriver(RequestNewWebDriverViewModel newWebDriverRequest, CancellationToken ct = default)
        {
            _logger.LogTrace("RequestNewDriver action executed.");

            RequestNewWebDriverResultViewModel result = await _supervisor.LeadslyRequestNewWebDriverAsync(newWebDriverRequest, ct);

            if(result.Succeeded == false)
            {
                return BadRequest_LeadslyCreateWebDriver(result.Failures);
            }

            return Ok(result.Value);
        }

        [HttpPost("connect")]
        [AllowAnonymous]
        public async Task<IActionResult> Connect(ConnectAccountViewModel connect, CancellationToken ct = default)
        {
            _logger.LogTrace("Connect action executed.");

            ConnectAccountResultViewModel result = await _supervisor.LeadslyAuthenticateUserAsync(connect, ct);

            if (result.Succeeded == false)
            {
                return BadRequest_LeadslyAuthenticationError(result.Failures);
            }

            return Ok(result.Value);
        }

        [HttpPost("connect/2fa")]
        [AllowAnonymous]
        public async Task<IActionResult> TwoFactorAuth(TwoFactorAuthViewModel twoFactorAuth, CancellationToken ct = default)
        {
            _logger.LogTrace("TwoFactorAuth action executed");

            TwoFactorAuthResultViewModel result = await _supervisor.LeadslyTwoFactorAuthAsync(twoFactorAuth, ct);

            if (result.Succeeded == false)
            {
                return BadRequest_LeadslyTwoFactorAuthError(result.Failures);
            }

            return Ok(result.Value);
        }

    }
}
