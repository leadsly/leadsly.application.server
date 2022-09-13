using Amazon.S3;
using Amazon.S3.Model;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class AwsS3Service : IAwsS3Service
    {
        private readonly IAmazonS3 _client;
        private readonly ILogger<AwsS3Service> _logger;
        private readonly S3BucketOptions _options;

        public AwsS3Service(IAmazonS3 client, IOptions<S3BucketOptions> options, ILogger<AwsS3Service> logger)
        {
            _client = client;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<bool> DeleteDirectoryAsync(string prefix, CancellationToken ct = default)
        {
            _logger.LogInformation("Deleting directory {0} from S3 bucket {1}", prefix, _options.Name);
            bool succeeded = false;
            try
            {
                ListObjectsRequest listObjectsRequest = new ListObjectsRequest
                {
                    BucketName = _options.Name,
                    Prefix = prefix + @"/"
                };

                ListObjectsResponse response = await _client.ListObjectsAsync(listObjectsRequest, ct);

                if (response.S3Objects.Count == response.MaxKeys)
                {
                    _logger.LogError("The delete operation may not delete all of the S3 bucket contents. Current request returned first 1000 items, there may be more items to be deleted.");
                    throw new NotImplementedException("The delete operation may not delete all of the S3 bucket contents. Current request returned first 1000 items, there may be more items to be deleted.");
                }

                DeleteObjectsRequest deleteObjectsRequest = new DeleteObjectsRequest()
                {
                    BucketName = _options.Name
                };

                // Process response.
                foreach (S3Object entry in response.S3Objects)
                {
                    deleteObjectsRequest.AddKey(entry.Key);
                }

                DeleteObjectsResponse deleteObjectsResponse = await _client.DeleteObjectsAsync(deleteObjectsRequest, ct);

                succeeded = deleteObjectsResponse.DeleteErrors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete directory {0} from S3 bucket {1}", prefix, _options.Name);
            }

            return succeeded;
        }
    }
}
