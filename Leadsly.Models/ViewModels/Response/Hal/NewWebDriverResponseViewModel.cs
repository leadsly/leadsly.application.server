﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Response.Hal
{
    public class NewWebDriverResponseViewModel : ResultBaseViewModel, INewWebDriverResponseViewModel
    {
        public string WebDriverId { get; set; }
    }
}
