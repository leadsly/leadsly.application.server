using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public class EnterTwoFactorAuthCodeRequest : LeadslyBaseRequest
    {
        public string RequestUrl { get; set; }
        public string TwoFactorAuthCode { get; set; }
        public string WebDriverId { get; set; }

    }
}
