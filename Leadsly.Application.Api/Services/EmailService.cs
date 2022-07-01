using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Leadsly.Domain;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Threading.Tasks;
using Leadsly.Domain.OptionsJsonModels;

namespace Leadsly.Application.Api.Services
{
    public class EmailService : IEmailService
    {
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions));
            _logger = logger;
        }

        private readonly IConfigurationSection _emailServiceOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public async Task<bool> SendEmailAsync(SendEmailRequest sendRequest)
        {
            bool succeeded;

            try
            {
                _logger.LogInformation("[EmailService]: Preparing to send email.");

                using (AmazonSimpleEmailServiceV2Client client = new AmazonSimpleEmailServiceV2Client(Amazon.RegionEndpoint.USEast1))
                {
                    _logger.LogInformation("[EmailService]: Sending email using Amazon SES...");
                    SendEmailResponse response = await client.SendEmailAsync(sendRequest);
                    _logger.LogInformation("[EmailService]: Email successfully sent");
                }

                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService]: Error occured while sending email.");

                succeeded = false;
            }

            return succeeded;
        }

        public SendEmailRequest ComposeEmail(ComposeEmailSettingsModel settings)
        {
            // Message to User
            SendEmailRequest sendRequest = new SendEmailRequest();

            // Add From
            sendRequest.FromEmailAddress = settings.From;

            // Add To         
            sendRequest.Destination = settings.Destination;

            // Add configuration set name
            sendRequest.ConfigurationSetName = settings.ConfigurationSetName;

            // Add Subject  
            sendRequest.Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content
                    {
                        Charset = "UTF-8",
                        Data = settings.Subject
                    },
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = settings.HtmlBody
                        },
                        Text = new Content
                        {
                            Charset = "UTF-8",
                            Data = settings.TextBody
                        }
                    }                   
                }                
            };

            return sendRequest;
        }

    }
}
