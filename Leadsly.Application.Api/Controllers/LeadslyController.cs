using Leadsly.Domain.Supervisor;
using Leadsly.Application.Model;
using Leadsly.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using Leadsly.Application.Model.ViewModels.Cloud;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using Leadsly.Application.Model.ViewModels;

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
        [AllowAnonymous]
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
        [AllowAnonymous]
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
