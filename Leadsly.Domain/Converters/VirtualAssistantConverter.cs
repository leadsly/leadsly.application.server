using Leadsly.Application.Model.Entities;
using Leadsly.Domain.ViewModels;

namespace Leadsly.Domain.Converters
{
    public static class VirtualAssistantConverter
    {
        public static VirtualAssistantViewModel Convert(VirtualAssistant virtualAssistant)
        {
            return new VirtualAssistantViewModel
            {
                HalId = virtualAssistant.HalId
            };
        }
    }
}
