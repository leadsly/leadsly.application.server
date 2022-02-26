﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Models.ViewModels
{
    [DataContract]
    public class AccountDetailsViewModel
    {
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "emailConfirmed", EmitDefaultValue = false)]
        public bool EmailConfirmed { get; set; }

        [DataMember(Name = "externalLogins", EmitDefaultValue = false)]
        public List<string> ExternalLogins { get; set; }

        [DataMember(Name = "hasAuthenticator", EmitDefaultValue = false)]
        public bool HasAuthenticator { get; set; }

        [DataMember(Name = "recoveryCodesLeft", EmitDefaultValue = false)]
        public int RecoveryCodesLeft { get; set; }

        [DataMember(Name = "twoFactorClientRemembered", EmitDefaultValue = false)]
        public bool TwoFactorClientRemembered { get; set; }

        [DataMember(Name = "twoFactorEnabled", EmitDefaultValue = false)]
        public bool TwoFactorEnabled { get; set; }
    }
}
