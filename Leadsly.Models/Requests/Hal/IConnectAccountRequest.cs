using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests.Hal
{
    public interface IConnectAccountRequest : IRequest
    {
        public string WebDriverId { get; }
        public string Username { get; }
        public string Password { get; }
        public string ConnectAuthUrl { get; }
    }
}
