using Leadsly.Models.Database;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Leadsly.Domain;

namespace Leadsly.Hal.Api.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user);
    }
}
