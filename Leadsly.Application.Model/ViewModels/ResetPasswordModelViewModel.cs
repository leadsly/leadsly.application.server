﻿using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class ResetPasswordModelViewModel
    {
        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password { get; set; }

        [DataMember(Name = "passwordResetToken", EmitDefaultValue = false)]
        public string PasswordResetToken { get; set; }
    }
}
