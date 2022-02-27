using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    [DataContract]
    public class NewWebDriverRequest : AccountBaseRequest, ISocialAccount
    {
        public SocialAccountDTO GetSocialAccountData()
        {
            return new()
            {
                AccountType = base.SocialAccountType,
                UserId = base.UserId,
                Username = base.Username
            };
        }
    }
}
