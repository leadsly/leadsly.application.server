using Leadsly.Domain.Models.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user);
    }
}
