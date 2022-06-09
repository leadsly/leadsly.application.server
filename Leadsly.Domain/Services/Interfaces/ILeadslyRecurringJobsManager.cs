using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface ILeadslyRecurringJobsManager
    {
        bool OnboardNewHalUnit(HalUnit halUnit);
    }
}
