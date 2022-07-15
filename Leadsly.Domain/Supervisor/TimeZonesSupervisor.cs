using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<IList<TimeZoneViewModel>> GetSupportedTimeZonesAsync(CancellationToken ct = default)
        {
            IList<LeadslyTimeZone> supportedTimeZones = await _timeZoneRepository.GetAllSupportedTimeZonesAsync(ct);

            if (supportedTimeZones == null)
            {
                return null;
            }

            IList<TimeZoneViewModel> timeZones = TimeZoneConverter.ConvertList(supportedTimeZones);
            return timeZones;
        }
    }
}
