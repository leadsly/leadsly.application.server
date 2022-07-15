using Leadsly.Application.Model;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Domain.Models.Entities;

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
                Username = model.Username,
                HalId = model.HalDetails.HalId
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
