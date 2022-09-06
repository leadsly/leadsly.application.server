using Leadsly.Domain;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    /// <summary>
    /// SocialAccount controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class SocialAccountsController : ApiControllerBase
    {
        public SocialAccountsController(ISupervisor supervisor, ILogger<SocialAccountsController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<SocialAccountsController> _logger;

        [HttpPatch("{socialAccountId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(string socialAccountId, [FromBody] JsonPatchDocument<SocialAccount> patchDoc, CancellationToken ct = default)
        {
            bool succeeded = await _supervisor.PatchUpdateSocialAccountAsync(socialAccountId, patchDoc, ct);
            if (succeeded == false)
            {
                return BadRequest(ProblemDetailsDescriptions.UpdateSocialAccountError);
            }
            return Ok();
        }
    }
}
