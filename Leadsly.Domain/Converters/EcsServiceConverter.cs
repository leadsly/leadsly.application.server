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
    public class EcsServiceConverter
    {
        public static EcsService Convert(EcsServiceDTO dto)
        {
            return new EcsService
            {
                AssignPublicIp = dto.AssignPublicIp,
                ClusterArn = dto.ClusterArn,
                CreatedAt = ((DateTimeOffset)dto.CreatedAt).ToUnixTimeSeconds(),
                CreatedBy = dto.CreatedBy,
                DesiredCount = dto.DesiredCount,
                EcsServiceRegistries = dto.Registries.Select(r => new EcsServiceRegistry
                {
                    RegistryArn = r.RegistryArn
                }).ToList(),
                SchedulingStrategy = dto.SchedulingStrategy,
                ServiceArn = dto.ServiceArn,
                ServiceName = dto.ServiceName,
                TaskDefinition = dto.TaskDefinition,
                UserId = dto.UserId
            };
        }
    }
}
