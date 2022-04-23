using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Campaigns.ProspectListsHandlers.ProspectLists
{
    public class ProspectListsCommand : ICommand
    {
        public ProspectListsCommand(IList<string> halIds)
        {
            HalIds = halIds;
        }

        public IList<string> HalIds { get; set; }
    }
}
