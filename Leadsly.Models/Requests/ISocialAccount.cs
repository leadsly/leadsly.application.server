using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public interface ISocialAccount
    {
        SocialAccountDTO GetSocialAccountData();
    }
}
