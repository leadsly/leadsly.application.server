using Leadsly.Domain;
using Leadsly.Models;
using System.Runtime.Serialization;

namespace Leadsly.Domain.ViewModels
{
    [DataContract]
    public class Disable2FaResultViewModel
    {
        [DataMember(Name = "status", EmitDefaultValue = true)]
        public TwoFactorAuthenticationStatus Status { get; set; }

        [DataMember(Name= "message", EmitDefaultValue = false)]
        public string Message { get; set; }
    }
}
