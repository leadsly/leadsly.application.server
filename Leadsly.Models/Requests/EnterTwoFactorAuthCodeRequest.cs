using Leadsly.Models.Requests.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public class EnterTwoFactorAuthCodeRequest : HalRequestBase, IEnterTwoFactorAuthCodeRequest
    {
        public string Code { get; set; }
        public string WebDriverId { get; set; }

    }
}
