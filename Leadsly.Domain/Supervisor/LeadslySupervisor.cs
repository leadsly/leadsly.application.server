using Leadsly.Domain.Models;
using Leadsly.Domain.ViewModels.LeadslyBot;
using Leadsly.Models;
using Leadsly.Models.Aws;
using Leadsly.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<LeadslyConnectionResult> ConnectAccountToLeadslyAsync(ConnectLeadslyViewModel connectAccount, CancellationToken ct = default)
        {
            await _leadslyBotApiService.ConnectToLeadslyAsync(connectAccount, ct);

            LeadslyConnectionResult result = new LeadslyConnectionResult();

            return result;
        }

        public async Task<LeadslySetupResultDTO> SetupLeadslyForUserAsync(LeadslySetupDTO setup, CancellationToken ct = default)
        {
            SocialAccountDTO socialAccountDTO = new()
            {
                AccountType = setup.SocialAccountType,
                Username = setup.Username,
                UserId = setup.UserId
            };

            // Try and get social account for this leadsly user, for the given username/email and socialAccountType
            SocialAccount socialAccount = await _leadslyProvider.GetSocialAccountAsync(socialAccountDTO, ct);
            
            // checks whether this email and social account type are setup with current users leadsly account
            if(socialAccount.ConfiguredWithUsersLeadslyAccount == false)
            {
                //TODO I don't know how this would ever be NOT null but more testing is required
                DockerContainerInfo existingContainerInfo = socialAccount.DockerContainerInfo;

                // this social username and social account type do not have a container available
                if (existingContainerInfo == null)
                {
                    SetupNewUserInLeadslyDTO setupUserInLeadsy = new SetupNewUserInLeadslyDTO
                    {
                        UserId = setup.UserId,
                        EcsService = new()
                        {
                            AssignPublicIp = PublicIp.Disabled,
                            ClusterArn = "arn:aws:ecs:us-east-1:709251838882:cluster/default",
                            DesiredCount = 1,
                            Subnets = new() { "subnet-049bf059384e86947", "subnet-0f41f5f070cc120b9" },
                            LaunchType = EcsLaunchType.Fargate,
                            SchedulingStrategy = "REPLICA",
                            ServiceName = "test-test",
                            TaskDefinition = "first-run-task-definition:1"

                        },
                        EcsTask = new()
                        {
                            AssignPublicIp = PublicIp.Enabled,
                            Cluster = "arn:aws:ecs:us-east-1:709251838882:cluster/default",
                            Count = 1,
                            LaunchType = EcsLaunchType.Fargate,
                            TaskDefinition = "first-run-task-definition:1",
                            Subnets = new() { "subnet-049bf059384e86947", "subnet-0f41f5f070cc120b9" }
                        }
                    };

                    CloudPlatformOperationResult awsOperation = await _cloudPlatformProvider.SetupNewUsersContainerAsync(setupUserInLeadsy);

                    if (awsOperation.Succeeded == false)
                    {

                    }
                    else
                    {
                        DockerContainerInfo newContainerInfo = (DockerContainerInfo)awsOperation.Value;

                    }

                }
            }
            else
            {               
                var res = await SetupExistingUserInLeadslyAsync(socialAccount, ct);
            }


            // if user has multiple containers figure out how to determine which container to use

            return new LeadslySetupResultDTO { };
        }


        private async Task<CloudPlatformOperationResult> SetupExistingUserInLeadslyAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            DockerContainerInfo dockerContainerInfo = await _containerRepository.GetContainerById(socialAccount.ContainerId, ct);

            SetupExistingUserInLeadslyDTO existingUser = new()
            {

            };

            // once we have docker container info run healthcheck

            var res = await _cloudPlatformProvider.SetupExistingContainerAsync(existingUser);

            return res;
        }
    }
}
