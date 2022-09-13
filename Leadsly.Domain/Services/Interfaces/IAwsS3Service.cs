using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IAwsS3Service
    {
        Task<bool> DeleteDirectoryAsync(string prefix, CancellationToken ct = default);
    }
}
