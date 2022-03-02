using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class FailureConverter
    {
        public static List<FailureViewModel> ConvertList(List<Failure> dtos)
        {
            return dtos.Select(dto => new FailureViewModel
            {
                Code = dto.Code ?? Codes.ERROR,
                Detail = dto.Detail,
                Reason = dto.Reason
            }).ToList();
        }
    }
}
