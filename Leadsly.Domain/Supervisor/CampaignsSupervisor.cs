using Leadsly.Domain.Converters;
using Leadsly.Domain.Models.Entities.Campaigns;
using Leadsly.Domain.Models.ViewModels.Campaigns;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<CampaignViewModel> UpdateCampaignAsync(string campaignId, JsonPatchDocument<Campaign> patchDoc, CancellationToken ct = default)
        {
            Campaign campaignToUpdate = await _campaignProvider.GetCampaignByIdAsync(campaignId, ct);

            patchDoc.ApplyTo(campaignToUpdate);

            campaignToUpdate = await _campaignProvider.UpdateCampaignAsync(campaignToUpdate, ct);
            if (campaignToUpdate == null)
            {
                return null;
            }

            CampaignViewModel campaignViewModel = CampaignConverter.Convert(campaignToUpdate);

            return campaignViewModel;
        }

        public async Task<CampaignViewModel> CloneCampaignAsync(string campaignId, CancellationToken ct = default)
        {
            throw new NotImplementedException();

            Campaign campaignToClone = await _campaignProvider.GetCampaignByIdAsync(campaignId, ct);

            if (campaignToClone == null)
            {
                return null;
            }

            Campaign newCampaign = campaignToClone.Copy();
            newCampaign.CampaignId = null;

            newCampaign = await _createCampaignService.CreateAsync(newCampaign, ct);
            CampaignViewModel viewModel = CampaignConverter.Convert(newCampaign);
            return viewModel;
        }

        public async Task<bool> DeleteCampaignAsync(string campaignId, CancellationToken ct = default)
        {
            return await _campaignProvider.DeleteCampaignAsync(campaignId, ct);
        }

        public async Task<CampaignViewModel> GetCampaignByIdAsync(string campaignId, CancellationToken ct = default)
        {
            Campaign campaign = await _campaignProvider.GetCampaignByIdAsync(campaignId, ct);
            if (campaign == null)
            {
                return null;
            }

            CampaignViewModel campaignViewModel = CampaignConverter.Convert(campaign);

            return campaignViewModel;
        }

        public async Task<CampaignsViewModel> GetCampaignsByUserIdAsync(string userId, CancellationToken ct = default)
        {
            CampaignsViewModel viewModel = new();

            IList<Campaign> campaigns = await _campaignProvider.GetAllByUserIdAsync(userId, ct);

            if (campaigns == null)
            {
                return viewModel;
            }

            foreach (Campaign campaign in campaigns)
            {
                CampaignViewModel vm = new();
                vm.Id = campaign.CampaignId;
                vm.Name = campaign.Name;
                vm.ConnectionsSentDaily = campaign.DailyInvites;
                vm.TotalConnectionsSent = await _campaignProvider.GetTotalConnectionsSentAsync(campaign.CampaignId, ct);
                vm.ConnectionsAccepted = await _campaignProvider.GetConnectionsAcceptedAsync(campaign.CampaignId, ct);
                vm.Replies = await _campaignProvider.GetRepliesAsync(campaign.CampaignId, ct);
                vm.ProfileViews = 0;
                vm.Active = campaign.Active;
                vm.Expired = campaign.Expired;
                vm.Notes = campaign.Notes;

                viewModel.Items.Add(vm);
            }

            return viewModel;
        }
    }
}
