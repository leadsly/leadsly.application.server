using System.Security.Claims;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Authentication
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedJwtAsync(string userId, ClaimsIdentity identity);
    }
}
