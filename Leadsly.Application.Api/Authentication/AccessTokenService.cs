﻿using Leadsly.Domain.ViewModels;
using Leadsly.Domain.Models;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Leadsly.Domain;
using Leadsly.Models.Entities;
using Leadsly.Application.Api.Authentication.Jwt;
using Leadsly.Api.Exceptions;

namespace Leadsly.Application.Api.Authentication
{
    public class AccessTokenService : IAccessTokenService
    {
        public AccessTokenService(IConfiguration configuration, IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions, IClaimsIdentityService claimsIdentityService, LeadslyUserManager userManager, RoleManager<IdentityRole> roleManager)
        {
            _configuration = configuration;
            _jwtFactory = jwtFactory;
            _jwtOptions = jwtOptions.Value;
            _claimsIdentityService = claimsIdentityService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private readonly IConfiguration _configuration;
        private readonly IJwtFactory _jwtFactory;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IClaimsIdentityService _claimsIdentityService;
        private readonly LeadslyUserManager _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private IJsonSerializer Serializer => new JsonNetSerializer();
        private IDateTimeProvider Provider => new UtcDateTimeProvider();
        private IBase64UrlEncoder UrlEncoder => new JwtBase64UrlEncoder();
        private IJwtAlgorithm Algorithm => new HMACSHA256Algorithm();

        public async Task<ApplicationAccessTokenViewModel> GenerateApplicationTokenAsync(string userId, ClaimsIdentity identity)
        {
            return new ApplicationAccessTokenViewModel
            {
                access_token = await _jwtFactory.GenerateEncodedJwtAsync(userId, identity),
                expires_in = (long)_jwtOptions.ValidFor.TotalSeconds
            };
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string expiredAccessToken)
        {
            IConfigurationSection jwtAppSettingOptions = _configuration.GetSection(nameof(JwtIssuerOptions));

            TokenValidationParameters expiredTokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._configuration[ApiConstants.VaultKeys.JwtSecret])),
                ValidateLifetime = false,

                RequireSignedTokens = true,
                RequireExpirationTime = true,
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = null;

            ClaimsPrincipal principal = tokenHandler.ValidateToken(expiredAccessToken, expiredTokenValidationParameters, out securityToken);
            JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new LeadslySecurityTokenException();
            }

            return principal;
        }

        public async Task<RenewAccessTokenResultViewModel> TryRenewAccessToken(string expiredAccessToken)
        {
            RenewAccessTokenResultViewModel result = new RenewAccessTokenResultViewModel();

            ClaimsPrincipal claimsPrincipal = GetPrincipalFromExpiredToken(expiredAccessToken);

            Claim userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return result;
            }

            ApplicationUser appUser = await _userManager.FindByIdAsync(userId.Value);

            if (appUser == null)
            {
                return result;
            }

            string refreshToken = await _userManager.GetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);

            bool isValid = await _userManager.VerifyUserTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.Purpose, refreshToken);

            if (isValid == false)
            {
                return result;
            }

            ClaimsIdentity claimsIdentity = await _claimsIdentityService.GenerateClaimsIdentityAsync(appUser);

            await _userManager.RemoveAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName);

            string newRefreshToken = await _userManager.GenerateUserTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.Purpose);

            IdentityResult settingNewTokenResult = await _userManager.SetAuthenticationTokenAsync(appUser, ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName, ApiConstants.DataTokenProviders.StaySignedInProvider.TokenName, newRefreshToken);

            if(settingNewTokenResult.Succeeded == false)
            {
                return result;
            }

            ApplicationAccessTokenViewModel accessToken = await GenerateApplicationTokenAsync(appUser.Id, claimsIdentity);

            result.Succeeded = true;

            result.AccessToken = accessToken;

            return result;
        }
    }    
}
