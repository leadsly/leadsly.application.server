﻿namespace Leadsly.Domain
{
    public static class ProblemDetailsDescriptions
    {
        public const string BadRequestDetail = "Some of the provided input data is invalid.";
        public const string BadRequest = "Some of the provided input data is invalid.";
        public const string ForbiddenDetail = "The server understood the request, but is refusing to authorize it.";
        public const string Forbidden = "The server understood the request, but is refusing to authorize it.";
        public const string InternalServerErrorDetail = "An unexpected error has occurred.";
        public const string InternalServerError = "An unexpected error has occurred.";
        public const string MethodNotAllowedDetail = "The request method is not supported by the target resource.";
        public const string MethodNotAllowed = "The request method is not supported by the target resource.";
        public const string NotAcceptableDetail = "The target resource cannot be returned in the media type requested.";
        public const string NotAcceptable = "The target resource cannot be returned in the media type requested.";
        public const string NotFoundDetail = "The requested resource cannot be found.";
        public const string NotFound = "The requested resource cannot be found.";
        public const string UnauthorizedDetail = "Authentication credentials are invalid.";
        public const string UnauthorizedRefreshTokenFailed = "Access token failed to refresh.";
        public const string UnauthorizedAccountLocked = "Account is locked.";
        public const string ForbiddenEmailNotConfirmed = "Email has not been confirmed.";
        public const string UnauthorizedExternalProvider = "Invalid external provider token";
        public const string UnauthorizedRegistrationToken = "Invalid registration token";
        public const string Unauthorized = "Authentication credentials are missing or invalid.";
        public const string ExpiredAccessTokenIsInvalid = "Expired access token is invalid.";
        public const string ExternalJwtIsInvalid = "External provider jwt is invalid.";
        public const string RegistrationErrorDetail = "User registration error occured.";
        public const string RegistrationDetail = "Missing or invalid registration data.";
        public const string UserNotFound = "User could not be found.";
        public const string TwoFactorAuthSetupVerificationCode = "Two factor authentication code is invalid.";
        public const string TwoStepVerificationCodeOrProviderIsInvalid = "Verification code or provider is invalid.";
        public const string TwoFactorAuthRecoveryCode = "Recovery code is invalid.";
        public const string CannotDisable2faWhenItsNotEnabled = "Cannot disable 2fa as it's not currently enabled.";
        public const string TwoFactorAuthenticationIsNotEnabled = "Two factor authentication is not enabled.";
        public const string FailedToDisable2fa = "An error occured while disabling 2fa.";
        public const string FailedToEnable2fa = "An error occured while enabling 2fa.";
        public const string FailedToResetAuthenticatorKey = "An error occured while resetting 2fa authenticator key.";
        public const string FailedToSendConfirmationEmail = "Failed to send confirmation email.";
        public const string FailedToSendChangeEmailLink = "Failed to send change email link.";
        public const string FailedToUpdatePassword = "Failed to update password.";
        public const string FailedToUpdateEmail = "Failed to update e-mail.";
        public const string FailedToUpdateResource = "Failed to update resource.";
        public const string FailedToConfirmUsersEmail = "Failed to confirm user's e-mail.";
        public const string FailedGetResource = "Failed to get resource.";
        public const string FailedToGenerateToken = "Failed to generate token.";
        public const string CampaignProspectsReplies = "Failed to process campaign prospects replies";
        public const string PotentialProspectsReplies = "Failed to process potential prospect replies.";
        public const string TokenNotFound = "Token was not found in the request.";
        public const string StripeCustomerDoesNotExist = "Customer does not exist.";
        public const string NoEmailFound_Stripe = "No email could be found.";
        public const string EventExtractionFailed_Stripe = "Could not determine webhook event type.";
        public const string NoTempPasswordValue = "No temporariy password value found.";
        public const string FailedToSendEmail = "Failed to send email.";
        public const string FailedToGetNetworkProspects = "Failed to get Network Prospects";
        public const string FailedToDeserialize = "Failed to deserialize object.";
        public const string CampaignNotFound = "Campaign not found.";
        public const string VirtualAssistant = "Failed to create Virtual Assistant";
        public const string ConnectLinkedInAccount = "Failed to link account to LinkedIn";
        public const string GetFollowUpMessages = "Failed to generate follow up messages";
        public const string RecentlyAddedProspects = "Failed to retrieve recently added prospects";
        public const string EnterTwoFactorAuthCode = "Failed to enter two factor auth code";
        public const string DeleteVirtualAssistant = "Failed to delete virtual assistant";
        public const string WebDriverCreationError = "Failed to create web driver";
        public const string AllActiveCampaigns = "Failed to retrieve active campaigns";
        public const string CreateCampaignError = "Failed to create new campaign";
        public const string CloneCampaignError = "Failed to clone campaign";
        public const string UpdateContactedCampaignProspects = "Failed to update contacted campaign prospects";
        public const string SentConnectionsUrlStatuses = "Getting sent connections url statuses";
        public const string UpdatingSentConnectionsUrlStatuses = "Updating sent connections url statuses";
        public const string LeadslySocialAccountAuthenticationError = "Failed to authenticate user's social account";
        public const string LeadslyTwoFactorAuthError = "Failed to perform two factor authentication for user's social account";
        public const string ProspectListError = "Error occured adding prospects";
        public const string ProcessSentFollowUpMessage = "Error processing sent follow up message";
        public const string UpdateProspectListPhaseError = "Error occured updating Prospect List phase";
        public const string UpdateSocialAccountError = "Error occured updating SocialAccount";
        public const string RepliedCampaignProspects = "Error occured updating campaign prospects that replied";
        public const string GeneralReportError = "Failed to retrieve general report data";
        public const string SearchUrlsProgress = "Failed to retrieve search urls progress";
    }
}
