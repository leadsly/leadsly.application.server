using Leadsly.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Infrastructure.Repositories
{
    public class FollowUpMessagePhaseRepository : IFollowUpMessagePhaseRepository
    {
        public FollowUpMessagePhaseRepository(DatabaseContext dbContext, ILogger<FollowUpMessagePhaseRepository> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        private readonly DatabaseContext _dbContext;
        private readonly ILogger<FollowUpMessagePhaseRepository> _logger;
    }
}
