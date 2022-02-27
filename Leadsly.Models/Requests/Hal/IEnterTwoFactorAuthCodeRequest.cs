using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public interface IEnterTwoFactorAuthCodeRequest : IRequest
    {
        public string Code { get; }
        public string WebDriverId { get; }
    }
}
