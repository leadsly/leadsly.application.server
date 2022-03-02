using Leadsly.Domain.Models;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.Entities;
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
