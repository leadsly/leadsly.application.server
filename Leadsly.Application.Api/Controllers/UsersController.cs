﻿using Amazon.SimpleEmailV2.Model;
using Leadsly.Application.Api.Services;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Domain;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    /// <summary>
    /// Users controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ApiControllerBase
    {
        public UsersController(
            IConfiguration configuration,
            LeadslyUserManager userManager,
            IEmailService emailService,
            IHtmlTemplateGenerator templateGenerator,
            SignInManager<ApplicationUser> signinManager,
            UrlEncoder urlEncoder,
            ISupervisor supervisor,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _templateGenerator = templateGenerator;
            _urlEncoder = urlEncoder;
            _signinManager = signinManager;
            _logger = logger;
            _supervisor = supervisor;


        }

        private readonly IConfiguration _configuration;
        private readonly LeadslyUserManager _userManager;
        private readonly SignInManager<ApplicationUser> _signinManager;
        private readonly UrlEncoder _urlEncoder;
        private readonly IEmailService _emailService;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<UsersController> _logger;
        private readonly ISupervisor _supervisor;



        /// <summary>
        /// Gets users account security details.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/account/security")]
        public async Task<IActionResult> SecurityDetails()
        {
            _logger.LogTrace("Security details action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);

            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                // return bad request user not founds
                return BadRequest_UserNotFound();
            }

            IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(appUser);

            string recoveryCodeString = await _userManager.GetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.AspNetUserProvider.ProviderName, ApiConstants.DataTokenProviders.AspNetUserProvider.TokenName);
            UserRecoveryCodesViewModel recoveryCodes = new UserRecoveryCodesViewModel
            {
                Items = recoveryCodeString != string.Empty ? recoveryCodeString?.Split(';') : Enumerable.Empty<string>()
            };

            AccountSecurityDetailsViewModel securityDetails = new AccountSecurityDetailsViewModel
            {
                ExternalLogins = logins.Select(l => l.ProviderDisplayName).ToList(),
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(appUser) != null,
                RecoveryCodes = recoveryCodes,
                TwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(appUser),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(appUser)
            };

            return Ok(securityDetails);
        }

        /// <summary>
        /// Gets users account general details.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/account/general")]
        public async Task<IActionResult> GeneralDetails()
        {
            _logger.LogTrace("General details action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                // return bad request user not found
                return BadRequest_UserNotFound();
            }

            AccountGeneralDetailsViewModel generalDetails = new AccountGeneralDetailsViewModel
            {
                Email = appUser.Email,
                Verified = appUser.EmailConfirmed
            };

            return Ok(generalDetails);
        }


        /// <summary>
        /// Resends email confirmation email.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/account/resend-email-verification")]
        public async Task<IActionResult> ResendEmailVerification()
        {
            _logger.LogTrace("Resend email verification action executed.");

            ApplicationUser appUser = await _userManager.GetUserAsync(User);

            if (appUser == null)
            {
                // return bad request user not found
                return BadRequest_UserNotFound();
            }

            string confirmEmailToken = await this._userManager.GenerateEmailConfirmationTokenAsync(appUser);

            if (confirmEmailToken == null)
            {
                _logger.LogDebug("ResendEmailVerification action failed to generate email confirmation token.");

                return BadRequest_FailedToGenerateToken();
            }

            string code = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(confirmEmailToken);

            // attempt to resend email here;
            IConfigurationSection clientOptions = this._configuration.GetSection(nameof(ClientOptions));
            string callBackUrl = ApiConstants.Email.Verify.Url.Replace(ApiConstants.Email.ClientAddress, clientOptions[nameof(ClientOptions.Address)]);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.IdParam, appUser.Id);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.EmailParam, appUser.Email);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.TokenParam, code);

            IConfigurationSection emailServiceOptions = this._configuration.GetSection(nameof(EmailServiceOptions));
            IConfigurationSection changeEmailOptions = emailServiceOptions.GetSection(nameof(EmailServiceOptions.VerifyEmail));
            string body = _templateGenerator.GenerateBodyFor(EmailTemplateTypes.VerifyEmail).Replace(ApiConstants.Email.CallbackUrlToken, callBackUrl);

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
                From = emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)],
                Subject = emailServiceOptions[nameof(EmailServiceOptions.VerifyEmail.EmailSubject)],
                HtmlBody = body,
                TextBody = "Please fill this part out before going to prod"
            });

            string email = appUser.Email;
            if (await _emailService.SendEmailAsync(sendRequest))
            {
                _logger.LogInformation("Email verification has been re-sent to: '{email}'", email);
                return Ok();
            }
            else
            {
                _logger.LogInformation("Email verification has failed to re-send to: '{email}'", email);
                return BadRequest_FailedToSendConfirmationEmail();
            }
        }

        /// <summary>
        /// Changes user's email.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/change-email-request")]
        public async Task<IActionResult> InitChangeEmailRequest(string id, [FromBody] ChangeEmailRequestViewModel model)
        {
            _logger.LogTrace("InitChangeEmailRequest action executed.");

            ApplicationUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                _logger.LogDebug("InitChangeEmailRequest action failed. Unable to find user by provided userId: {id}.", id);

                return BadRequest_FailedToUpdateEmail();
            }

            if (await _userManager.CheckPasswordAsync(appUser, model.Password) == false)
            {
                _logger.LogDebug("InitChangeEmailRequest action failed to verify user's password.");

                return BadRequest_FailedToUpdateEmail();
            }

            string changeEmailToken = await _userManager.GenerateChangeEmailTokenAsync(appUser, model.NewEmail);

            if (changeEmailToken == null)
            {
                _logger.LogDebug("InitChangeEmailRequest action failed to generate change email token.");

                return BadRequest_FailedToGenerateToken();
            }

            string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(changeEmailToken);

            IConfigurationSection clientOptions = this._configuration.GetSection(nameof(ClientOptions));
            string callBackUrl = ApiConstants.Email.Change.Url.Replace(ApiConstants.Email.ClientAddress, clientOptions[nameof(ClientOptions.Address)]);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.IdParam, appUser.Id);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.EmailParam, model.NewEmail);
            callBackUrl = callBackUrl.Replace(ApiConstants.Email.TokenParam, token);

            IConfigurationSection emailServiceOptions = this._configuration.GetSection(nameof(EmailServiceOptions));
            IConfigurationSection changeEmail = emailServiceOptions.GetSection(nameof(EmailServiceOptions.ChangeEmail));
            string body = _templateGenerator.GenerateBodyFor(EmailTemplateTypes.ChangeEmail).Replace(ApiConstants.Email.CallbackUrlToken, callBackUrl);

            // send registration email
            SendEmailRequest sendRequest = _emailService.ComposeEmail(new ComposeEmailSettingsModel
            {
                Destination = new Destination
                {
                    ToAddresses = new List<string>
                    {
                       model.NewEmail
                    }
                },
                From = emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)],
                Subject = changeEmail[nameof(EmailServiceOptions.ChangeEmail.EmailSubject)],
                HtmlBody = body,
                TextBody = "Please fill this part out before going to prod"
            });

            if (await _emailService.SendEmailAsync(sendRequest))
            {
                string email = model.NewEmail;
                _logger.LogInformation("Change email link has been sent to: '{email}'", email);

                return NoContent();
            }
            else
            {
                _logger.LogDebug("Failed to send change email link.");

                return BadRequest_FailedToSendChangeEmailConfirmation();
            }
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("{id}/email")]
        public async Task<IActionResult> ChangeEmail(string id, [FromBody] ChangeEmailViewModel model)
        {
            _logger.LogTrace("ChangeEmail action executed.");

            ApplicationUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                _logger.LogDebug("ChangeEmail action failed. Unable to find user by provided userId: {id}.", id);

                return BadRequest_FailedToUpdateEmail();
            }

            string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(model.Token);

            appUser.UserName = model.NewEmail;
            IdentityResult result = await this._userManager.ChangeEmailAsync(appUser, model.NewEmail, token);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("ChangeEmail action failed. Unable to change user's email. New email or token is invalid.");

                return BadRequest_FailedToUpdateEmail();
            }

            return NoContent();
        }

        /// <summary>
        /// Verifies user's email.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailViewModel model)
        {
            _logger.LogTrace("ConfirmEmail action executed.");

            ApplicationUser appUser = await _userManager.FindByEmailAsync(model.Email);

            if (appUser == null)
            {
                string email = model.Email;
                _logger.LogDebug("ConfirmEmail action failed. Unable to find user by provided email: {email}.", email);

                return BadRequest_UserNotFound();
            }

            if (model.Token == null)
            {
                _logger.LogDebug("ConfirmEmail action failed. ConfirmEmail token not found in the request.");

                return BadRequest_TokenNotFound();
            }

            string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(model.Token);

            IdentityResult result = await _userManager.ConfirmEmailAsync(appUser, token);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("ConfirmEmail action failed. Failed to confirm user's email. Email or token is invalid.");

                return BadRequest_FailedToConfirmUsersEmail();
            }

            return NoContent();
        }

        /// <summary>
        /// Changes user's password.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("{id}/password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] PasswordChangeViewModel model)
        {
            _logger.LogTrace("ChangePassword action executed.");

            ApplicationUser appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null)
            {
                _logger.LogDebug("ChangePassword action failed. Unable to find user by provided userId: {id}.", id);

                return BadRequest_FailedToUpdatePassword();
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(appUser, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("Password reset operation failed to change user's password.");

                return BadRequest_PasswordNotUpdated(result.Errors);
            }

            return NoContent();
        }

        /// <summary>
        /// Checks if email has already been registered.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("emails/exists")]
        public async Task<IActionResult> EmailExists([FromQuery] string email)
        {
            _logger.LogTrace("EmailExists action executed. Looking for: {email}", email);

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            return user == null ? new JsonResult(false) : new JsonResult(true);
        }

        /// <summary>
        /// Gets user's subscriptions.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}/subscriptions")]
        public async Task<IActionResult> UserSubscriptions([FromRoute] string userId)
        {
            _logger.LogTrace("UserSubscriptions executed. Looking for: {userId}", userId);

            ApplicationUser user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Failed to find any user with {userId}.", userId);

                return BadRequest_UserNotFound();
            }

            // fetch user's subscriptions from stripe


            return Ok();
        }

        /// <summary>
        /// Checks if email has generated by the server and not yet created.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("emails/invited")]
        public async Task<IActionResult> EmailInvited([FromQuery] string email)
        {
            _logger.LogTrace("EmailInvited action executed. Looking for: {email}", email);

            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("[EmailInvited]: User has not been generated by the server. This usually means user who has not paid is trying to register.");

                return new JsonResult(new AccountInviteViewModel { Error = true, ErrorDescription = "You must be invited to register." });
            }

            if (user.EmailConfirmed == true)
            {
                _logger.LogWarning("[EmailInvited]: User has already registered.");

                return new JsonResult(new AccountInviteViewModel { Error = true, ErrorDescription = "User already registered." });
            }

            return new JsonResult(new AccountInviteViewModel { Error = false });

        }

        /// <summary>
        /// Resets user's password.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordModelViewModel model)
        {
            _logger.LogTrace("ResetPassword action executed.");

            ApplicationUser userToResetPassword = await _userManager.FindByIdAsync(id);

            if (userToResetPassword == null)
            {
                _logger.LogDebug("ResetPassword action failed. Unable to find user by provided userId: {id}.", id);

                return BadRequest_FailedToUpdatePassword();
            }

            if (model.PasswordResetToken == null)
            {
                _logger.LogDebug("ResetPassword action failed. Password reset token not found in the request.");

                return BadRequest_TokenNotFound();
            }

            string code = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(model.PasswordResetToken);

            IdentityResult result = await _userManager.ResetPasswordAsync(userToResetPassword, code, model.Password);

            if (result.Succeeded == false)
            {
                _logger.LogDebug("ResetPassword action failed. Password reset operation failed to update the new password.");

                return BadRequest_PasswordNotUpdated(result.Errors);
            }

            return NoContent();
        }

        /// <summary>
        /// Emails user a password reset link.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            // TODO consider adding client id to all of the calls to allow for single backend api and multiple client apps

            _logger.LogTrace("ForgotPassword action executed. Generating reset password link for: '{email}'", email);

            ApplicationUser userToRecoverPassword = await _userManager.FindByEmailAsync(email);

            // We want to fail silently
            if (userToRecoverPassword == null)
            {
                _logger.LogDebug("User not found.");

                return NoContent();
            }

            string passwordResetCode = await _userManager.GeneratePasswordResetTokenAsync(userToRecoverPassword);

            if (passwordResetCode == null)
            {
                _logger.LogDebug("Failed to generate password reset token");

                return NoContent();
            }

            string code = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(passwordResetCode);

            IConfigurationSection clientOptions = this._configuration.GetSection(nameof(ClientOptions));
            string callBackUrl = $"{clientOptions[nameof(ClientOptions.Address)]}/auth/reset-password?userId={userToRecoverPassword.Id}&code={code}";

            IConfigurationSection emailServiceOptions = this._configuration.GetSection(nameof(EmailServiceOptions));
            IConfigurationSection forgotPasswordEmail = emailServiceOptions.GetSection(nameof(EmailServiceOptions.PasswordReset));
            string body = _templateGenerator.GenerateBodyFor(EmailTemplateTypes.PasswordReset).Replace(ApiConstants.Email.CallbackUrlToken, callBackUrl);

            // send registration email
            SendEmailRequest sendRequest = _emailService.ComposeEmail(new ComposeEmailSettingsModel
            {
                Destination = new Destination
                {
                    ToAddresses = new List<string>
                    {
                       email
                    }
                },
                From = emailServiceOptions[nameof(EmailServiceOptions.SystemAdminEmail)],
                Subject = emailServiceOptions[nameof(EmailServiceOptions.PasswordReset.EmailSubject)],
                HtmlBody = body,
                TextBody = "Please fill this part out before going to prod"
            });

            if (await _emailService.SendEmailAsync(sendRequest))
            {
                _logger.LogInformation("Password recovery email has been sent to: '{email}'", email);

                return NoContent();
            }

            return NoContent();
        }
    }
}
