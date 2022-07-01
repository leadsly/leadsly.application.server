using Leadsly.Application.Model.Aws.DTOs;
using Leadsly.Application.Model.Entities;

namespace Leadsly.Domain.Converters
{
    public class CloudMapDiscoveryServiceConverter
    {
        public static CloudMapDiscoveryService Convert(CloudMapDiscoveryServiceDTO dto)
        {
            return new CloudMapDiscoveryService
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
