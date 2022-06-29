using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Cloud;
using Leadsly.Application.Model.ViewModels.Response;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{

    /// <summary>
    /// Leadsly controller.
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

        /// <summary>
        /// Gets applications name and version. If returned indicates api is successfully running.
        /// </summary>
        /// <returns></returns>
        [HttpGet("connected-account")]
        public async Task<IActionResult> GetConnectedAccountAsync()
        {
            _logger.LogTrace("ConnectedAccount action executed.");

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            HalOperationResultViewModel<IOperationResponseViewModel> connectedAccount = await _supervisor.GetConnectedAccountAsync<IOperationResponseViewModel>(userId);

            // TODO we need to handle situation when something fails and return Bad_Request();
            return Ok(connectedAccount);
        }

        [HttpPost("setup")]
        public async Task<IActionResult> SetupAccountWithLeadsly(SetupAccountViewModel setupLeasdsly, CancellationToken ct = default)
        {
            _logger.LogTrace("SetupUserWithLeadsly action executed.");

            HalOperationResultViewModel<IOperationResponseViewModel> result = await _supervisor.LeadslyAccountSetupAsync<IOperationResponseViewModel>(setupLeasdsly, ct);
            LeadslySetup setup = result.Data as LeadslySetup;

            if (result.OperationResults.Succeeded == false && setup.RequiresNewCloudResource == false)
            {
                _logger.LogTrace("[SetupLeadslyForUserAsync] did not succeeded.");
                if (result.OperationResults.Failures.Count > 0)
                {
                    return BadRequest_LeadslySetup(result.OperationResults.Failures);
                }
                else
                {
                    return BadRequest_LeadslySetup();
                }
            }

            return Ok(result);
        }

        [HttpPost("webdriver")]
        public async Task<IActionResult> RequestNewDriver(NewWebDriverRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("RequestNewDriver action executed.");

            HalOperationResultViewModel<INewWebDriverResponseViewModel> result = await _supervisor.LeadslyRequestNewWebDriverAsync<INewWebDriverResponseViewModel>(request, ct);

            if (result.OperationResults.Succeeded == false)
            {
                return BadRequest_LeadslyCreateWebDriver(result.OperationResults.Failures);
            }

            return Ok(result.Value);
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect(ConnectAccountRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("Connect action executed.");

            HalOperationResultViewModel<IConnectAccountResponseViewModel> result = await _supervisor.LeadslyAuthenticateUserAsync<IConnectAccountResponseViewModel>(request, ct);

            // problem details from hal
            if (result.OperationResults.ProblemDetails != null)
            {
                return ProblemDetailsResult(result.OperationResults.ProblemDetails);
            }

            if (result.OperationResults.Succeeded == false)
            {
                return BadRequest_LeadslyAuthenticationError(result.OperationResults.Failures);
            }

            return Ok(result);
        }

        [HttpPost("connect/2fa")]
        public async Task<IActionResult> TwoFactorAuth(TwoFactorAuthRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("TwoFactorAuth action executed");

            HalOperationResultViewModel<IEnterTwoFactorAuthCodeResponseViewModel> result = await _supervisor.LeadslyTwoFactorAuthAsync<IEnterTwoFactorAuthCodeResponseViewModel>(request, ct);

            if (result.OperationResults.Succeeded == false)
            {
                return BadRequest_LeadslyTwoFactorAuthError(result.OperationResults.Failures);
            }

            return Ok(result);
        }

    }
}
