using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Repositories
{
    public interface IContainerRepository
    {
        Task<List<DockerContainerInfo>> GetContainersByUserId(string userId, CancellationToken ct = default);
        Task<DockerContainerInfo> GetContainerById(string id, CancellationToken ct = default);
    }
}
