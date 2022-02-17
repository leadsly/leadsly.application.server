using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    public class NewSocialAccountResult
    {
        public bool Succeeded { get; set; }
        public SocialAccount Value { get; set; }
        public List<FailureDTO> Failures { get; set; } = new();
    }
}
