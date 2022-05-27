using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Entities.Campaigns.Phases;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
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
            HalOperationResultViewModel<IOperationResponseViewModel> result = await _supervisor.PatchUpdateSocialAccountAsync<IOperationResponseViewModel>(socialAccountId, patchDoc, ct);
            if(result.OperationResults.Succeeded == false)
            {
                return BadRequest_UpdatingSocialAccount(result.OperationResults.Failures);
            }
            return Ok();
        }
    }
}
