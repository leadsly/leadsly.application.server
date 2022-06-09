using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain
{
    public interface IRecurringJobsHandler
    {
        public Task CreateAndPublishJobsAsync();

        public Task CreateAndPublishJobsByHalIdAsync(string halId);
    }
}
