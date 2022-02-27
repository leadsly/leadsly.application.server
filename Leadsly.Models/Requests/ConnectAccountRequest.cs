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
    public class ConnectAccountRequest : AccountBaseRequest, ISocialAccount
    {
        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }

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
