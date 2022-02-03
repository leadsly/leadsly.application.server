using MimeKit;

namespace Leadsly.Hal.Api.Services
{
    public interface IEmailService
    {
        bool SendEmail(MimeMessage message);

        MimeMessage ComposeEmail(ComposeEmailSettingsModel settings);
    }
}
