using Leadsly.Domain.Supervisor;
using Leadsly.Models;
using Leadsly.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Leadsly.Models.ViewModels.Hal;
using Leadsly.Models.ViewModels.Interfaces;
using Leadsly.Models.ViewModels.Cloud;

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

            HalOperationResult<INewWebDriverResponse> result = await _supervisor.LeadslyRequestNewWebDriverAsync<INewWebDriverResponse>(newWebDriverRequest, ct);

            if (result.Succeeded == false)
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

            HalOperationResult<IConnectAccountResponse> result = await _supervisor.LeadslyAuthenticateUserAsync<IConnectAccountResponse>(connect, ct);

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

            HalOperationResult<IEnterTwoFactorAuthCodeResponse> result = await _supervisor.LeadslyTwoFactorAuthAsync<IEnterTwoFactorAuthCodeResponse>(twoFactorAuth, ct);

            if (result.Succeeded == false)
            {
                return BadRequest_LeadslyTwoFactorAuthError(result.Failures);
            }

            return Ok(result.Value);
        }

    }
}
