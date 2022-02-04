using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels;
using Leadsly.Models.Entities;
using System.Linq;

namespace Leadsly.Domain.Converters
{
    static class ApplicationUserConverter
    {
        public static ApplicationUserViewModel Convert(ApplicationUser user)
        {
            return new ApplicationUserViewModel
            {
                Id = user.Id,
                Email = user.Email
            };
        }
    }
}
