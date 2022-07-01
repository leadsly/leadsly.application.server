namespace Leadsly.Domain.OptionsJsonModels
{
    public class EmailServiceOptions
    {
        public string SystemAdminEmail { get; set; }

        public class PasswordReset
        {
            public string EmailSubject { get; set; }
        }

        public class VerifyEmail
        {
            public string EmailSubject { get; set; }
        }

        public class ChangeEmail
        {
            public string EmailSubject { get; set; }
        }

        public class RegisterEmail
        {
            public string EmailSubject { get; set; }
        }
    }
}
