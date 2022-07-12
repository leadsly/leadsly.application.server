using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Domain.Models.ViewModels.Reports;
using Leadsly.Domain.Models.ViewModels.Reports.ApexCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public async Task<GeneralReportViewModel> GetGeneralReportAsync(string userId, CancellationToken ct = default)
        {
            IList<Campaign> campaigns = await _campaignProvider.GetAllByUserIdAsync(userId, ct);
            GeneralReportViewModel generalReport = new()
            {
                SelectedCampaignId = campaigns.FirstOrDefault()?.CampaignId
            };

            IList<ChartDataApexViewModel> chartData = await ToApexChartData(campaigns);
            generalReport.Items = chartData.ToList();
            generalReport.DateFilters = new()
            {
                PrepopulatedFilters = new()
                {
                    { "Last 7 Days", DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds() }
                }
            };

            return generalReport;
        }

        public async Task<IList<ChartDataApexViewModel>> ToApexChartData(IList<Campaign> campaigns, CancellationToken ct = default)
        {
            IList<ChartDataApexViewModel> chartData = new List<ChartDataApexViewModel>();
            // create a list of strings from the last 7 days
            List<DateTimeOffset> dates = new List<DateTimeOffset>();
            for (int i = 0; i < 7; i++)
            {
                dates.Add(DateTimeOffset.Now.AddDays(-i));
            }

            foreach (Campaign campaign in campaigns)
            {
                Dictionary<GeneralReporKeys, Dictionary<string, IList<long>>> reportData = new Dictionary<GeneralReporKeys, Dictionary<string, IList<long>>>()
                {
                    {
                        GeneralReporKeys.ConnectionsSent,
                        new Dictionary<string, IList<long>>()
                    },
                    {
                        GeneralReporKeys.ConnectionsAccepted,
                        new Dictionary<string, IList<long>>()
                    },
                    {
                        GeneralReporKeys.Replies,
                        new Dictionary<string, IList<long>>()
                    }
                };

                foreach (CampaignProspect prospect in campaign.CampaignProspectList?.CampaignProspects)
                {
                    DateTimeOffset connectionSentDate = await _timestampService.GetDateFromTimestampLocalizedAsync(campaign.HalId, prospect.ConnectionSentTimestamp, ct);
                    // check if connectionSentDate falls between dates
                    if (dates.Any(d => d == connectionSentDate.Date))
                    {
                        string date = dates.Find(d => d == connectionSentDate.Date).ToString("MM/dd/yyyy");
                        if (!reportData.ContainsKey(GeneralReporKeys.ConnectionsSent))
                        {

                            reportData[GeneralReporKeys.ConnectionsSent] = new Dictionary<string, IList<long>>()
                            {
                                {
                                    date, new List<long>()
                                    {
                                        prospect.ConnectionSentTimestamp
                                    }
                                }
                            };

                        }
                        else
                        {
                            reportData[GeneralReporKeys.ConnectionsSent][date].Add(prospect.ConnectionSentTimestamp);
                        }
                    }

                    DateTimeOffset replyReceivedDate = await _timestampService.GetDateFromTimestampLocalizedAsync(campaign.HalId, prospect.RepliedTimestamp, ct);
                    if (dates.Any(d => d == replyReceivedDate))
                    {
                        string date = dates.Find(d => d == connectionSentDate.Date).ToString("MM/dd/yyyy");
                        if (!reportData.ContainsKey(GeneralReporKeys.Replies))
                        {

                            reportData[GeneralReporKeys.Replies] = new Dictionary<string, IList<long>>()
                            {
                                {
                                    date, new List<long>()
                                    {
                                        prospect.RepliedTimestamp
                                    }
                                }
                            };

                        }
                        else
                        {
                            reportData[GeneralReporKeys.Replies][date].Add(prospect.AcceptedTimestamp);
                        }
                    }

                    DateTimeOffset connectionAcceptedDate = await _timestampService.GetDateFromTimestampLocalizedAsync(campaign.HalId, prospect.AcceptedTimestamp, ct);
                    if (dates.Any(d => d == connectionAcceptedDate))
                    {
                        string date = dates.Find(d => d == connectionSentDate.Date).ToString("MM/dd/yyyy");
                        if (!reportData.ContainsKey(GeneralReporKeys.ConnectionsAccepted))
                        {

                            reportData[GeneralReporKeys.ConnectionsAccepted] = new Dictionary<string, IList<long>>()
                            {
                                {
                                    date, new List<long>()
                                    {
                                        prospect.AcceptedTimestamp
                                    }
                                }
                            };

                        }
                        else
                        {
                            reportData[GeneralReporKeys.ConnectionsAccepted][date].Add(prospect.AcceptedTimestamp);
                        }
                    }
                }

                chartData.Add(new ChartDataApexViewModel
                {
                    CampaignId = campaign.CampaignId,
                    XAxis = new()
                    {
                        Type = "datetime",
                        Categories = dates.Select(d => d.ToString("MM/dd/yyyy")).ToList()
                    },
                    Series = new()
                    {
                        new()
                        {
                            Name = "Connections Sent",
                            Data = reportData[GeneralReporKeys.ConnectionsSent]?.Select(x => (object)x.Value.Count()).ToList() ?? new List<object>()
                        },
                        new()
                        {
                            Name = "Connections Accepted",
                            Data = reportData[GeneralReporKeys.ConnectionsAccepted]?.Select(x => (object)x.Value.Count()).ToList() ?? new List<object>()
                        },
                        new()
                        {
                            Name = "Replies",
                            Data = reportData[GeneralReporKeys.Replies]?.Select(x => (object)x.Value.Count()).ToList() ?? new List<object>()
                        }
                    }
                });
            }



            return chartData;
        }

        public enum GeneralReporKeys
        {
            None,
            ConnectionsSent,
            ConnectionsAccepted,
            Replies
        }
    }
}
