﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class UpdateSearchUrlDetailsRequest : BaseHalRequest
    {
        [DataMember(Name = "SearchUrlDetailsRequests")]
        public IList<SearchUrlDetailsRequest> SearchUrlDetailsRequests { get; set; }
    }
}