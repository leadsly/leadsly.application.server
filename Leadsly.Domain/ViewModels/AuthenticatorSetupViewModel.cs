﻿using System.Runtime.Serialization;

namespace Leadsly.Domain.ViewModels
{
    [DataContract]
    public class AuthenticatorSetupViewModel
    {
        [DataMember(Name = "sharedKey", EmitDefaultValue = false)]
        public string SharedKey { get; set; }
        [DataMember(Name = "authenticatorUri", EmitDefaultValue = false)]
        public string AuthenticatorUri { get; set; }
    }
}
