using Leadsly.Models;
using Leadsly.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    [DataContract]
    public class TwoFactorAuthRequest : AccountBaseRequest, ISocialAccount
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        public SocialAccountDTO GetSocialAccountData()
        {
            return new()
            {
                AccountType = base.SocialAccountType,
                Username = base.Username,
                UserId = base.UserId
            };
        }
    }
}
