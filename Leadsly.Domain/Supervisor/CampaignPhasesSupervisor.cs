using Leadsly.Application.Model;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.ViewModels;
using Leadsly.Application.Model.ViewModels.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    partial class Supervisor : ISupervisor
    {

        public async Task<HalOperationResult<T>> ProcessProspectsAsync<T>(ProspectListPhaseCompleteRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IEnumerable<PrimaryProspect> prospects = await _campaignRepository.CreatePrimaryProspectsAsync(request.Prospects, ct);
            if(prospects == null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public async Task<HalOperationResult<T>> ProcessNewMyNetworkConnectionsAsync<T>(MyNetworkNewConnectionsRequest request, CancellationToken ct = default)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // grab each new connection and determine which campaign it came from            

            // to do this we have to get all active campaigns for the given hal machine

            // then check each campaigns prospect list and see if this individual exists there

            // if user exists there grab the first follow up message

            // parse replace any tokens (name)

            // send send the follow up message message to hal to 
            // result = await _campaignPhaseFacade.ProcessNewNetworkConnectionsAsync<T>(request.NewConnectionProspects, request.HalId, ct);

            if(result.Succeeded == false)
            {
                return result;
            }

            return result;
            // if succeeded queue up follow up messages
        }
    }
}
