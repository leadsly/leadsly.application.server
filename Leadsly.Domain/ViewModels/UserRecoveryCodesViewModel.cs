using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Domain.ViewModels
{
    [DataContract]
    public class UserRecoveryCodesViewModel
    {
        [DataMember(Name = "items", EmitDefaultValue = false)]
        public IEnumerable<string> Items { get; set; }
    }
}
