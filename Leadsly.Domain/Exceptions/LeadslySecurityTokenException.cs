using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace Leadsly.Domain.Exceptions
{
    public class LeadslySecurityTokenException : SecurityTokenException, ILeadslyApiException
    {
        public string Type => ProblemDetailsTypes.InternalServerErrorType;

        public string Title => ReasonPhrases.GetReasonPhrase(500);

        public int Status => StatusCodes.Status500InternalServerError;

        public string Detail => ProblemDetailsDescriptions.ExpiredAccessTokenIsInvalid;

        public string Instance => "/api/auth/refresh-token";
    }
}
