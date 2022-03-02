using Leadsly.Application.Model;
using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;
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
