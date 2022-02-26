using Leadsly.Models.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class ResultBase : IOperationResult
    {
        [DataMember(IsRequired = true)]
        public bool Succeeded { get; set; }        
        [DataMember]
        public List<Failure> Failures { get; set; } = new();
    }
}
