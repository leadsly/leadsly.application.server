using Leadsly.Application.Api.Authentication;
using Leadsly.Application.Api.Extensions;
using Leadsly.Application.Api.Services;
using Leadsly.Application.Domain.OptionsJsonModels;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    /// <summary>
    /// Authentication controller.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ApiControllerBase
    {
        public AuthController(
            IAccessTokenService tokenService,
            IClaimsIdentityService claimsIdentityService,
            IConfiguration configuration,
            LeadslyUserManager userManager,
            UrlEncoder urlEncoder,
            IEmailService emailService,
            IHtmlTemplateGenerator templateGenerator,
            ILogger<AuthController> logger)
        {
            _tokenService = tokenService;
            _claimsIdentityService = claimsIdentityService;
            _userManager = userManager;
            _configuration = configuration;
            _urlEncoder = urlEncoder;
            _emailService = emailService;
            _templateGenerator = templateGenerator;
            _emailServiceOptions = configuration.GetSection(nameof(EmailServiceOptions));
            _logger = logger;
        }

        private readonly IAccessTokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly UrlEncoder _urlEncoder;
        private readonly IClaimsIdentityService _claimsIdentityService;
        private readonly LeadslyUserManager _userManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _emailServiceOptions;
        private readonly IHtmlTemplateGenerator _templateGenerator;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Signs user in.
        /// </summary>
        /// <param name="signin"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("signin")]
        public async Task<IActionResult> Signin([FromBody] SigninUserViewModel signin, CancellationToken ct = default)
        {
            _logger.LogTrace("Signin action executed.");

            if (string.IsNullOrEmpty(signin.Password))
            {
                _logger.LogDebug("Request was missing user's password.");

                return Unauthorized_InvalidCredentials();
            }

            ApplicationUser appUser = await _userManager.FindByEmailAsync(signin.Email);

            if (appUser == null)
            {
                _logger.LogDebug("User not found.");

                return Unauthorized_InvalidCredentials();
            }

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            if (await _userManager.IsEmailConfirmedAsync(appUser) == false)
            {
                //[CONFIRMATION-WALL]: Keep code if email confirmation is required.
                //await RequireConfirmedEmail(appUser);
            }

            if (await _userManager.CheckPasswordAsync(appUser, signin.Password) == false)
            {
                _logger.LogDebug("Password validation failed.");

                await _userManager.AccessFailedAsync(appUser);

                if (await _userManager.IsLockedOutAsync(appUser))
                {
                    _logger.LogDebug("User is locked out.");

                    return Unauthorized_AccountLockedOut();
                }

                int failedAttempts = await _userManager.GetAccessFailedCountAsync(appUser);

                _logger.LogDebug("Failed sign in attempt number: '{failedAttempts}'.", failedAttempts);

                return Unauthorized_InvalidCredentials(failedAttempts);
            }

            if (await _userManager.GetTwoFactorEnabledAsync(appUser) == true)
            {
                var providers = await _userManager.GetValidTwoFactorProvidersAsync(appUser);
                if (providers.Contains("Authenticator") == false)
                {
                    return BadRequest_TwoFactorAuthenticationIsNotEnabled();
                }

                return Ok(new AuthResponseViewModel
                {
                    Is2StepVerificationRequired = true,
                    Provider = "Authenticator"
                });
            }

            // if user successfully logged in reset access failed count back to zero.
            await _userManager.ResetAccessFailedCountAsync(appUser);

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

            ApplicationAccessTokenViewModel accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

            return Ok(new AuthResponseViewModel
            {
                AccessToken = accessToken
            });
        }
                
        /// <summary>
        /// Signs user up.
        /// </summary>
        /// <param name="signupModel"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupUserViewModel signupModel, [FromQuery] string registrationToken, CancellationToken ct = default)
        {
            _logger.LogTrace("Signup action executed.");

            if (string.Equals(signupModel.Password, signupModel.ConfirmPassword, StringComparison.OrdinalIgnoreCase) == false)
            {
                _logger.LogDebug("Confirm password and password do not match.");

                return BadRequest_UserRegistrationError();
            }

            ApplicationUser appUser = await _userManager.FindByEmailAsync(signupModel.Email);

            if (ct.IsCancellationRequested)
                ct.ThrowIfCancellationRequested();

            if (appUser == null)
            {
                string email = signupModel.Email;
                _logger.LogDebug("Could not find user with email: {email}.", email);

                return BadRequest_UserNotFound();
            }

            if (appUser.EmailConfirmed == true)
            {
                _logger.LogError("User has already registered.");

                return BadRequest_UserRegistrationError();
            }
            else
            {
                string token = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Decode(registrationToken);

                bool validationResult = await _userManager.VerifyUserTokenAsync(appUser, ApiConstants.DataTokenProviders.RegisterNewUserProvider.ProviderName, ApiConstants.DataTokenProviders.RegisterNewUserProvider.Purpose, token);

                if (validationResult == true)
                {
                    await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.RegisterNewUserProvider.ProviderName, ApiConstants.DataTokenProviders.RegisterNewUserProvider.TokenName);

                    IdentityResult result = await _userManager.ChangePasswordAsync(appUser, this._configuration["TempPassword"], signupModel.Password);

                    if (result.Succeeded == false)
                    {
                        _logger.LogDebug("User registration data is invalid or missing.");

                        return BadRequest_UserNotCreated(result.Errors);
                    }


                    // if you dont want to sign user in after signing up, comment out rest of this code.            
                    ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

                    ApplicationAccessTokenViewModel accessToken = await _tokenService.GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

                    // TODO may cause issues if StaySignedIn token was not previously set. NEEDS TO BE TESTED
                    _logger.LogWarning("NEEDS TO BE TESTED. IF NEW USER DOES NOT HAVE STAYSIGNEDIN TOKEN DOES CALLING RemoveAuthenticationTokenAsync CAUSE ISSUES?");
                    await SetOrRefreshStaySignedInToken(appUser, _userManager, _logger);

                    // set email confirmed falg to true
                    appUser.EmailConfirmed = true;
                    await _userManager.UpdateAsync(appUser);


                    return Ok(accessToken);
                }
            }            

            _logger.LogDebug("Registration token was invalid.");

            return Unauthorized_InvalidRegistrationToken();
        }

        /// <summary>
        /// Refreshes user's authentication token.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            _logger.LogTrace("RefreshToken action executed.");

            RenewAccessTokenResultViewModel result = new RenewAccessTokenResultViewModel();

            string expiredAccessToken = HttpContext.GetAccessToken();

            if (expiredAccessToken == string.Empty)
            {
                _logger.LogWarning("Expired access token not present on the reqest.");

                return Unauthorized_AccessTokenRefreshFailed();
            }

            try
            {
                // request has token but it failed authentication. Attempt to renew the token
                result = await _tokenService.TryRenewAccessToken(expiredAccessToken);
                bool succeeded = result.Succeeded;
                _logger.LogDebug("Attempted to rewnew jwt. Result: {succeeded}.", succeeded);                
            }
            catch (Exception ex)
            {
                // Silently fail
                _logger.LogError(ex, "Failed to rewnew jwt.");

                return Unauthorized_AccessTokenRefreshFailed();
            }

            if (result.Succeeded == false)
            {
                _logger.LogError("Failed to rewnew jwt.");

                return Unauthorized_AccessTokenRefreshFailed();
            }

            return Ok(result.AccessToken);
        }

        /// <summary>
        /// Signs user out.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpDelete]
        [AllowAnonymous]
        [Route("signout")]
        public async Task<IActionResult> Signout(CancellationToken ct = default)
        {
            _logger.LogTrace("Signout action executed.");

            Claim userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                ApplicationUser appUser = await _userManager.FindByIdAsync(userIdClaim.Value);

                if (appUser != null)
                {
                    await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);
                }
            }

            return Ok();
        }

    }
}
