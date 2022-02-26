using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels
{
    public class RenewAccessTokenResultViewModel
    {
        public bool Succeeded { get; set; } = false;
        public ApplicationAccessTokenViewModel AccessToken { get; set; }
    }
}
