using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels.Stripe
{
    [DataContract]
    public class CustomerViewModel
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }        
        
    }

}
