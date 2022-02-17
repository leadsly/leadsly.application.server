using Leadsly.Models;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public class EcsTaskDefinitionConverter
    {
        public static EcsTaskDefinition Convert(EcsTaskDefinitionDTO dto)
        {
            return new EcsTaskDefinition
            {
                ContainerName = dto.ContainerName,
                Family = dto.Family
            };
        }
    }
}
