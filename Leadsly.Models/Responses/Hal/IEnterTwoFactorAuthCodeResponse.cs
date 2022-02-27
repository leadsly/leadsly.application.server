using Leadsly.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Responses.Hal
{
    public interface IEnterTwoFactorAuthCodeResponse : IOperationResponse
    {
        public bool InvalidOrExpiredCode { get; set; }
        public bool DidUnexpectedErrorOccur { get; set; }
    }
}
