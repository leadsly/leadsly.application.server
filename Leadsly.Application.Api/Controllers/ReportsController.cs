using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportsController : ApiControllerBase
    {
        public ReportsController(ILogger<ReportsController> logger, ISupervisor supervisor)
        {
            logger = _logger;
            supervisor = _supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<ReportsController> _logger;

        
    }
}
