using Leadsly.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using Leadsly.Domain;
using Leadsly.Models.Database;

namespace Leadsly.Application.Api.Authentication
{
    public interface IClaimsIdentityService
    {
        Task<ClaimsIdentity> GenerateClaimsIdentityAsync(ApplicationUser user);
    }
}
