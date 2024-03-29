﻿using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class ChangeEmailViewModel
    {
        [DataMember(Name = "newEmail", EmitDefaultValue = false)]
        public string NewEmail { get; set; }

        [DataMember(Name = "token", EmitDefaultValue = false)]
        public string Token { get; set; }
    }
}
