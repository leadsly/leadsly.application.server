using Leadsly.Domain.Supervisor;
using Leadsly.Shared.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leadsly.Hal.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LeadslyBotController : ApiControllerBase
    {
        public LeadslyBotController(ILogger<LeadslyBotController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ILogger<LeadslyBotController> _logger;
        private readonly ISupervisor _supervisor;

        [HttpPost("account/connect")]
        public async Task<IActionResult> ConnectUser()
        {
            // if(user has live container)
            // authenticate user with linked in

            // if (user does not have a live container)
            // create a container for the user
            // then authenticate the user
            return Ok();
        }
    }
}
