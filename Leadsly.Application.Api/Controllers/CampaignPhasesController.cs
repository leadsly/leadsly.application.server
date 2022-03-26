using Leadsly.Api;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
using Leadsly.Application.Model.Requests;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CampaignPhasesController : ApiControllerBase
    {
        public CampaignPhasesController(ILogger<CampaignPhasesController> logger, ISupervisor supervisor)
        {
            _logger = logger;
        }

        private readonly ILogger<CampaignPhasesController> _logger;
        private readonly ISupervisor _supervisor;

        /// <summary>
        /// This is for processing newly accepted connections on my network page
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("new-my-network-connections")]
        public async Task<IActionResult> ProcessNewMyNetWorkConnections([FromBody] MyNetworkNewConnectionsRequest newConnectionProspects)
        {
            return Ok();
        }
    }
}
