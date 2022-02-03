using Amazon.SimpleEmailV2.Model;
using MimeKit;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(SendEmailRequest sendRequest);

        SendEmailRequest ComposeEmail(ComposeEmailSettingsModel settings);
    }
}
