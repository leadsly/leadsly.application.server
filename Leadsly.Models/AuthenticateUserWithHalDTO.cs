using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class AuthenticateUserWithHalDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
