using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.Requests
{
    public class ConnectUserAccountRequest : LeadslyBaseRequest
    {
        public string RequestUrl { get; set; }
        public string WebDriverId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectAuthUrl { get; set; }
    }
}
