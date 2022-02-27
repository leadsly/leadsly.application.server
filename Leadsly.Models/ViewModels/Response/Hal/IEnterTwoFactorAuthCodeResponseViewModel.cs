using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Response.Hal
{
    public interface IEnterTwoFactorAuthCodeResponseViewModel : IOperationResponseViewModel
    {
        public bool InvalidOrExpiredCode { get; set; }
        public bool DidUnexpectedErrorOccur { get; set; }
    }
}
