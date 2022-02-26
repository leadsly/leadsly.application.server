using Leadsly.Models;
using Leadsly.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Hal
{
    [DataContract]
    public class ConnectAccountViewModel : RequestForHalBaseViewModel, ISocialAccount
    {
        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }

        public SocialAccountDTO GetSocialAccountData()
        {
            return new()
            {
                AccountType = SocialAccountType,
                UserId = UserId,
                Username = Username
            };
        }
    }
}
