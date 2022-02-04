using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class ConnectAccountTwoFactorAuthDTO : ConnectAccountDTO
    {
        public string Code { get; set; }
    }
}
