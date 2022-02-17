using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public class SocialAccountConverter
    {
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
