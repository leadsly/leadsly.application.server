using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.ViewModels.VirtualAssistant;

namespace Leadsly.Domain.Converters
{
    public static class VirtualAssistantConverter
    {
        public static VirtualAssistantViewModel Convert(VirtualAssistant virtualAssistant)
        {
            return new VirtualAssistantViewModel
            {
                VirtualAssistantId = virtualAssistant.VirtualAssistantId,
                HalId = virtualAssistant.HalId,
                TimezoneId = virtualAssistant.HalUnit.TimeZoneId
            };
        }
    }
}
