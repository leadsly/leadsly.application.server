using Leadsly.Domain;
using Leadsly.Domain.Models.Requests;
using Leadsly.Domain.Models.ViewModels.VirtualAssistant;
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
    [Route("virtual-assistant")]
    public class VirtualAssistantController : ApiControllerBase
    {
        public VirtualAssistantController(ISupervisor supervisor, ILogger<VirtualAssistantController> logger)
        {
            _supervisor = supervisor;
            _logger = logger;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<VirtualAssistantController> _logger;

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateVirtualAssistantRequest request, CancellationToken ct = default)
        {
            _logger.LogTrace("Create action executed.");

            VirtualAssistantViewModel virtualAssistant = await _supervisor.CreateVirtualAssistantAsync(request, ct);

            return virtualAssistant == null ? BadRequest_CreateVirtualAssistant() : Ok(virtualAssistant);
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync(CancellationToken ct = default)
        {
            _logger.LogTrace("GetAsync action executed.");
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            VirtualAssistantInfoViewModel virtualAssistant = await _supervisor.GetVirtualAssistantInfoAsync(userId, ct);

            return virtualAssistant == null ? BadRequest_GetResource() : Ok(virtualAssistant);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(CancellationToken ct)
        {
            _logger.LogTrace("Delete action executed.");
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            DeleteVirtualAssistantViewModel deleteVirtualAssistant = await _supervisor.DeleteVirtualAssistantAsync(userId, ct);

            return deleteVirtualAssistant == null ? BadRequest(ProblemDetailsDescriptions.DeleteVirtualAssistant) : Ok(deleteVirtualAssistant);
        }

    }
}
