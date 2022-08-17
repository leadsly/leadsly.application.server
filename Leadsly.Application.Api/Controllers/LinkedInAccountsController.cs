using Leadsly.Domain;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.LinkedInAccount;
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
    [Route("linkedin-accounts/{userId}")]
    public class LinkedInAccountsController : ApiControllerBase
    {
        public LinkedInAccountsController(ISupervisor supervisor, ILogger<LinkedInAccountsController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<LinkedInAccountsController> _logger;

        /// <summary>
        /// Whether user is connected to LinkedIn.
        /// </summary>
        /// <returns></returns>
        [HttpGet("info")]
        public async Task<IActionResult> GetConnectedAccountAsync(string userId, CancellationToken ct = default)
        {
            _logger.LogTrace("GetConnectedAccountAsync action executed.");

            ConnectedViewModel connected = await _supervisor.GetConnectedAccountAsync(userId, ct);

            return Ok(connected);
        }

        [HttpPost("connect")]
        public async Task<IActionResult> ConnectAsync(ConnectLinkedInAccountRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("ConnectAsync action executed.");

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ConnectLinkedInAccountResultViewModel result = await _supervisor.LinkLinkedInAccount(request, userId, HttpContext.Response.Headers, HttpContext.Request.Headers, ct);

            if (result == null)
            {
                return BadRequest_ConnectLinkedInAccount();
            }

            return Ok(result);
        }

        [HttpPost("2fa")]
        public async Task<IActionResult> TwoFactorAuthAsync(TwoFactorAuthRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("TwoFactorAuthAsync action executed");

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            TwoFactorAuthResultViewModel result = await _supervisor.EnterTwoFactorAuthAsync(userId, request, ct);

            return result == null ? BadRequest(ProblemDetailsDescriptions.EnterTwoFactorAuthCode) : Ok(result);
        }

        [HttpPost("email-challenge-pin")]
        public async Task<IActionResult> EmailChallengePinAsync(EmailChallengePinRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("EmailChallengePinAsync action executed");

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            EmailChallengePinResultViewModel result = await _supervisor.EnterEmailChallengePinAsync(userId, request, ct);

            return result == null ? BadRequest(ProblemDetailsDescriptions.EnterTwoFactorAuthCode) : Ok(result);
        }

        // "disconnect" 

    }
}
