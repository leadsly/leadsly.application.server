using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectList
{
    public class ProspectListCommand : ICommand
    {
        public ProspectListCommand(string prospectListPhaseId, string userId)
        {
            ProspectListPhaseId = prospectListPhaseId;
            UserId = userId;
        }

        public string ProspectListPhaseId { get; set; }
        public string UserId { get; set; }
    }
}
