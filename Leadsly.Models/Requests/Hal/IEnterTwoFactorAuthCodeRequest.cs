using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public interface IEnterTwoFactorAuthCodeRequest : IHalRequest
    {
        public string Code { get; }
        public string WebDriverId { get; }
    }
}
