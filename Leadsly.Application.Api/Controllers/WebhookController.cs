using Amazon.SimpleEmailV2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stripe;
using System.IO;
using Leadsly.Domain.Supervisor;
using Leadsly.Models.Database;
using Leadsly.Domain;
using Leadsly.Application.Api.Services;
using Leadsly.Application.Api.OptionsJsonModels;
using Leadsly.Shared.Api;

namespace Leadsly.Application.Api.Controllers
{
    /// <summary>
    /// Stripe controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]    
    public class WebhookController : ApiControllerBase
    {
        public WebhookController(
            IConfiguration configuration,                        
            IEmailService emailService,
            IHtmlTemplateGenerator templateGenerator,
            LeadslyUserManager userManager,
            ISupervisor supervisor,
            ILogger<WebhookController> logger)
        {
            _configuration = configuration;            
            _emailService = emailService;
            _templateGenerator = templateGenerator;
            _emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions));
            _clientOptions = configuration.GetSection(nameof(ClientOptions));
            _userManager = userManager;
            _supervisor = supervisor;
            _logger = logger;
        }

        
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ISupervisor _supervisor;
        private readonly LeadslyUserManager _userManager;
        private readonly IConfiguration _emailServiceOptions;
        private readonly IConfiguration _clientOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<WebhookController> _logger;

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Event stripeEvent = null;
            string json = string.Empty;
            try
            {
                json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _configuration[ApiConstants.VaultKeys.StripeWebhookSecret]) ?? throw new StripeException();
                
            }            
            catch(StripeException ex)
            {
                _logger.LogError(ex, "Error occured extracting event data from stripe request.");

                return BadRequest_EventExtractionFailed_Stripe();
            }

            string stripeEvt = stripeEvent.Type;
            if(stripeEvent.Type == Events.CustomerCreated)
            {
                _logger.LogInformation("Stripe responded with {stripeEvt}.", stripeEvt);

                return await OnCustomerCreated_Stripe(stripeEvent);
            }

            _logger.LogTrace("Webhook from stripe executed {stripeEvt}, an event we are not listening for.", stripeEvt);

            return NoContent();
        }

        [NonAction]
        private async Task<IActionResult> OnCustomerCreated_Stripe(Event stripeEvent)
        {
            Customer stripeCustomerFromEvent = stripeEvent.Data.Object as Customer;

            if (stripeCustomerFromEvent == null)
            {
                string stripeCustomerId = stripeCustomerFromEvent?.Id;
                _logger.LogError("Stripe customer {stripeCustomerId} does not exist.", stripeCustomerId);

                return BadRequest_StripeCustomerDoesNotExist();
            }

            // ensure order contains email
            if (stripeCustomerFromEvent.Email == null)
            {
                string customerId = stripeCustomerFromEvent?.Id;
                _logger.LogError("Stripe customer {customerId} does not contain email.", customerId);

                return BadRequest_NoEmailStripe();
            }

            Customer_Stripe customerStripe = new Customer_Stripe
            {
                Customer = stripeCustomerFromEvent.Id
            };

            customerStripe = await _supervisor.AddCustomerAsync_Stripe(customerStripe);

            // create user with the e-mail and TempPassword
            ApplicationUser appUser = new ApplicationUser
            {
                Email = stripeCustomerFromEvent.Email,
                UserName = stripeCustomerFromEvent.Email,
                Customer_Stripe = customerStripe
            };

            string tempPassword = this._configuration[nameof(ApiConstants.VaultKeys.TempPassword)];
            if (tempPassword == null || tempPassword == string.Empty)
            {
                _logger.LogError("Could not retrieve temporary user password. Please ensure value exists for 'TempPassword' in your security vault.");

                return BadRequest_NoTempPasswordValue();
            }

            IdentityResult result = await _userManager.CreateAsync(appUser, tempPassword);

            if (result.Succeeded == false)
            {
                _logger.LogError("Failed to setup user's account.");

                return BadRequest_UserNotCreated(result.Errors);
            }

            // generate user's registration token
            string registrationToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.RegisterNewUserProvider.ProviderName, ApiConstants.DataTokenProviders.RegisterNewUserProvider.Purpose);
            bool succeeded = registrationToken != null;
            if (succeeded == false)
            {
                _logger.LogError("Failed to generate user's registration token.");

                return BadRequest_FailedToGenerateToken();
            }

            IdentityResult setRegistrationTokenResult = await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RegisterNewUserProvider.ProviderName, ApiConstants.DataTokenProviders.RegisterNewUserProvider.TokenName, registrationToken);
            if (setRegistrationTokenResult.Succeeded == false)
            {
                _logger.LogError("Failed to save generated registration token.");

                return BadRequest_FailedToGenerateToken();
            }

            string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(registrationToken);

            string fromEmail = _emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)];
            IConfiguration registerEmailConfiguration = _emailServiceOptions.GetSection(nameof(EmailServiceOptions.RegisterEmail));
            string clientUrl = _clientOptions[nameof(ClientOptions.Address)];
            string signupUrl = _clientOptions[nameof(ClientOptions.SignUpUrl)];

            // send registration email
            SendEmailRequest sendRequest = _emailService.ComposeEmail(new ComposeEmailSettingsModel
            {
                Destination = new Destination
                {
                    ToAddresses = new List<string>
                    {
                       appUser.Email
                    }
                },
                From = fromEmail,
                Subject = registerEmailConfiguration[nameof(EmailServiceOptions.RegisterEmail.EmailSubject)],
                HtmlBody =
                            $@"<html>
                                <head></head>
                                    <body>
                                        <h3>Please follow the link to register {clientUrl}{signupUrl}?registrationToken={token}</h3>
                                        <p></p>
                                    </body>
                            </html>",
                TextBody = "Welcome! Lets build your network together"
            });

            bool success = await _emailService.SendEmailAsync(sendRequest);

            if (success == false)
            {
                return BadRequest_FailedToSendEmail();
            }

            return Ok();
        }
    }
}
