using Leadsly.Domain;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.Campaigns;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("users/{userId}/[controller]")]
    public class CampaignsController : ApiControllerBase
    {
        public CampaignsController(ILogger<CampaignsController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<CampaignsController> _logger;

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(string _, CancellationToken ct = default)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            CampaignsViewModel response = await _supervisor.GetCampaignsByUserIdAsync(userId, ct);

            if (response == null)
            {
                return BadRequest(ProblemDetailsDescriptions.AllActiveCampaigns);
            }

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id, CancellationToken ct = default)
        {
            CampaignViewModel response = await _supervisor.GetCampaignByIdAsync(id, ct);

            if (response == null)
            {
                BadRequest_CampaignNotFound();
            }

            return Ok(response);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] JsonPatchDocument<Campaign> patchDoc, CancellationToken ct = default)
        {
            CampaignViewModel patchedCampaign = await _supervisor.UpdateCampaignAsync(id, patchDoc, ct);

            if (patchedCampaign == null)
            {
                return BadRequest_FailedToUpdateResource();
            }

            return Ok(patchedCampaign);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateCampaignRequest request, CancellationToken ct = default)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            CampaignViewModel response = await _supervisor.CreateCampaignAsync(request, userId, ct);
            if (response == null)
            {
                return BadRequest(ProblemDetailsDescriptions.CreateCampaignError);
            }

            return Ok(response);
        }

        [HttpPost("{id}/clone")]
        public async Task<IActionResult> CloneAsync(string id, CancellationToken ct = default)
        {
            CampaignViewModel response = await _supervisor.CloneCampaignAsync(id, ct);
            if (response == null)
            {
                return BadRequest(ProblemDetailsDescriptions.CloneCampaignError);
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id, CancellationToken ct = default)
        {
            await _supervisor.DeleteCampaignAsync(id, ct);

            return NoContent();
        }

    }
}
