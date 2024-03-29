﻿using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public static class WebDriverConverter
    {
        public static INewWebDriverResponseViewModel Convert(INewWebDriverResponse response)
        {
            return new NewWebDriverResponseViewModel
            {
                WebDriverId = response.WebDriverId
            };
        }
    }
}
