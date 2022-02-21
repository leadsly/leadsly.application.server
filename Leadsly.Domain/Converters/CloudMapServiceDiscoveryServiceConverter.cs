using Leadsly.Models;
using Leadsly.Models.Aws.DTOs;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Converters
{
    public class CloudMapServiceDiscoveryServiceConverter
    {
        public static CloudMapServiceDiscoveryService Convert(CloudMapServiceDiscoveryServiceDTO dto)
        {
            return new CloudMapServiceDiscoveryService
            {
                Arn = dto.Arn,
                ServiceDiscoveryId = dto.ServiceDiscoveryId,
                Name = dto.Name,
                NamespaceId = dto.NamespaceId,
                CreateDate = dto.CreateDate
            };
        }
    }
}
