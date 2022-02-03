﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Models
{
    [DataContract]
    public class CreateContainerResultDTO
    {
        public bool ServiceSuccessfullyCreated { get; set; }
        public bool TaskSuccessfullyCreated { get; set; }
        /// <summary>
        /// Any potential failures for any tasks that were executed.
        /// </summary>
        public List<FailureDTO> Failures { get; set; }
        public ECSServiceDTO ECSService { get; set; }
        public List<ECSTaskDTO> ECSTasks  { get; set; }
    }
}
