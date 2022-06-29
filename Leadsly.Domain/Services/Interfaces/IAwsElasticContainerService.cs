using Amazon.ECS.Model;
using Leadsly.Application.Model.Aws.ElasticContainerService;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IAwsElasticContainerService
    {
        Task<CreateServiceResponse> CreateServiceAsync(CreateEcsServiceRequest createServiceRequest, CancellationToken ct = default);
        Task<DeleteServiceResponse> DeleteServiceAsync(DeleteEcsServiceRequest deleteServiceRequest, CancellationToken ct = default);
        Task<string> RollbackServiceAsync(CancellationToken ct = default);
        Task<RunTaskResponse> RunTaskAsync(RunEcsTaskRequest request, CancellationToken ct = default);
        Task<StopTaskResponse> StopTaskAsync(StopEcsTaskRequest request, CancellationToken ct = default);
        Task<RegisterTaskDefinitionResponse> RegisterTaskDefinitionAsync(RegisterEcsTaskDefinitionRequest registerTaskDefinitionRequest, CancellationToken ct = default);
        Task<DeregisterTaskDefinitionResponse> DeregisterTaskDefinitionAsync(DeregisterEcsTaskDefinitionRequest deregisterTaskDefinitionRequest, CancellationToken ct = default);
        Task<string> RollbackTaskDefinitionRegistrationAsync(CancellationToken ct = default);
        Task<UpdateServiceResponse> UpdateServiceAsync(UpdateEcsServiceRequest updateServiceRequest, CancellationToken ct = default);
        Task<DescribeServicesResponse> DescribeServicesAsync(DescribeEcsServicesRequest describeService, CancellationToken ct = default);
        Task<ListTasksResponse> ListTasksAsync(ListEcsTasksRequest listEcsTasksRequest, CancellationToken ct = default);
        Task<DescribeTasksResponse> DescribeTasksAsync(DescribeEcsTasksRequest describeEcsTasksRequest, CancellationToken ct = default);
        Task<DescribeServicesResponse> DescribeServices(DescribeEcsServicesRequest describeEcsServicesRequest, CancellationToken ct = default);
    }
}
