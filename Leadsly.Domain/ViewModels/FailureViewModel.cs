using Leadsly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.ViewModels
{
    public class FailureViewModel
    {
        public Codes? Code { get; set; }
        public string Detail { get; set; }
        public string Reason { get; set; }
    }
}
