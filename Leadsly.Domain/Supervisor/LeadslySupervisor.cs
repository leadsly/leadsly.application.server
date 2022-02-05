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

            // check if this username/email with this social account type already exists
            SocialAccount socialAccount = await _leadslyProvider.GetSocialAccountAsync(socialAccountDTO, ct);

            // should always either be null or zero or 

            if(socialAccount.Connected == false)
            {
                DockerContainerInfo existingContainerInfo = await _leadslyProvider.GetContainerInfoBySocialAccountAsync(socialAccountDTO, ct);

                // this social username and social account type do not have a container available
                if (existingContainerInfo == null)
                {
                    SetupNewUserInLeadslyDTO setupUserInLeadsy = new SetupNewUserInLeadslyDTO
                    {
                        UserId = setup.UserId,
                        EcsService = new()
                        {
                            
                        },
                        EcsTask = new()
                        {

                        }
                    };

                    AwsOperationResult awsOperation = await _awsElasticContainerProvider.SetupNewUsersContainerAsync(setupUserInLeadsy);

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

        private async Task<AwsOperationResult> SetupExistingUserInLeadslyAsync(SocialAccount socialAccount, CancellationToken ct = default)
        {
            DockerContainerInfo dockerContainerInfo = await _containerRepository.GetContainerById(socialAccount.ContainerId, ct);

            SetupExistingUserInLeadslyDTO existingUser = new()
            {

            };

            // once we have docker container info run healthcheck

            var res = await _awsElasticContainerProvider.SetupExistingContainerAsync(existingUser);

            return res;
        }
    }
}
