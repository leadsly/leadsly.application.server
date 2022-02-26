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
    public class RequestNewWebDriverViewModel : RequestForHalBaseViewModel, ISocialAccount
    {
        public SocialAccountDTO GetSocialAccountData()
        {
            return new()
            {
                AccountType = SocialAccountType,
                Username = Username,
                UserId = UserId
            };
        }
    }
}
