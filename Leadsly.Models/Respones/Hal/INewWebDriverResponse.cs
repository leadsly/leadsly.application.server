﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models.ViewModels.Interfaces
{
    public interface INewWebDriverResponse : IOperationResponse
    {
        public string WebDriverId { get; }
    }
}
