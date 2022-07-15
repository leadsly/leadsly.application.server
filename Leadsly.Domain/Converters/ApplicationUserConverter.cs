using Leadsly.Application.Model.ViewModels;
using Leadsly.Domain.Models.Entities;

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
