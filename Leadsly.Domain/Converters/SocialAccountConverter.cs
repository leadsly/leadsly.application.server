using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public class SocialAccountConverter
    {
        public static SocialAccountViewModel Convert(SocialAccount model)
        {
            return new SocialAccountViewModel
            {
                AccountType = model.AccountType,
                MonthlySearchLimitReached = model.MonthlySearchLimitReached,
                SocialAccountId = model.SocialAccountId,
                UserId = model.UserId,
                Username = model.Username
            };
        }
        public static SocialAccount Convert(SocialAccountDTO dto)
        {
            return new SocialAccount
            {
                AccountType = dto.AccountType,
                UserId = dto.UserId,
                Username = dto.Username
            };
        }
    }
}
