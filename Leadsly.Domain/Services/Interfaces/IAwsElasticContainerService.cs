using Amazon.ECS.Model;
using Leadsly.Application.Model.Aws.ElasticContainerService;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IAwsElasticContainerService
    {
        Task<CreateServiceResponse> CreateServiceAsync(CreateServiceRequest request, CancellationToken ct = default);
        Task<DeleteServiceResponse> DeleteServiceAsync(DeleteEcsServiceRequest deleteServiceRequest, CancellationToken ct = default);
        Task<bool> DeleteServiceAsync(string serviceName, string clusterName, CancellationToken ct = default);
        Task<bool> DeleteTaskDefinitionRegistrationAsync(string taskDefinitionFamily, CancellationToken ct = default);
        Task<RunTaskResponse> RunTaskAsync(RunEcsTaskRequest request, CancellationToken ct = default);
        Task<StopTaskResponse> StopTaskAsync(StopEcsTaskRequest request, CancellationToken ct = default);
        Task<RegisterTaskDefinitionResponse> RegisterTaskDefinitionAsync(RegisterTaskDefinitionRequest request, CancellationToken ct = default);
        Task<DeregisterTaskDefinitionResponse> DeregisterTaskDefinitionAsync(DeregisterEcsTaskDefinitionRequest deregisterTaskDefinitionRequest, CancellationToken ct = default);
        Task<UpdateServiceResponse> UpdateServiceAsync(UpdateEcsServiceRequest updateServiceRequest, CancellationToken ct = default);
        Task<DescribeServicesResponse> DescribeServicesAsync(DescribeServicesRequest request, CancellationToken ct = default);
        Task<ListTasksResponse> ListTasksAsync(ListTasksRequest request, CancellationToken ct = default);
        Task<DescribeTasksResponse> DescribeTasksAsync(DescribeEcsTasksRequest describeEcsTasksRequest, CancellationToken ct = default);
        Task<DescribeServicesResponse> DescribeServices(DescribeEcsServicesRequest describeEcsServicesRequest, CancellationToken ct = default);
    }
}
