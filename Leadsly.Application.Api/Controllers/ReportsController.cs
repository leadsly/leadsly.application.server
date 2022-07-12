using Leadsly.Domain;
using Leadsly.Domain.Models.ViewModels.Reports;
using Leadsly.Domain.Models.ViewModels.Reports.ApexCharts;
using Leadsly.Domain.Supervisor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportsController : ApiControllerBase
    {
        public ReportsController(ILogger<ReportsController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<ReportsController> _logger;

        [HttpGet("general")]
        public async Task<IActionResult> GetGeneralReportDataAsync(CancellationToken ct = default)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            GeneralReportViewModel response = await _supervisor.GetGeneralReportAsync(userId, ct);
            if (response == null)
            {
                return BadRequest(ProblemDetailsDescriptions.GeneralReportError);
            }

            return Ok(response);
        }

        private static Random rnd = new Random();
        protected static GeneralReportViewModel CampaignsEffectivenessReports { get; set; } = new GeneralReportViewModel
        {
            SelectedCampaignId = "3",
            Items = new List<ChartDataApexViewModel>
            {
                new ChartDataApexViewModel
                {
                    CampaignId = "1",
                    XAxis = new ApexXAxisViewModel
                        {
                            Type = "datetime",
                            Categories = new List<string>
                            {
                                "01/01/2022",
                                "01/02/2022",
                                "01/03/2022",
                                "01/04/2022",
                                "01/05/2022",
                                "01/06/2022",
                                "01/07/2022"
                            }
                        },
                    Series = new List<ApexAxisChartSeriesViewModel>
                    {
                            new()
                            {
                                Name = "Connections Sent",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Connections Accepted",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Replies",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Profile Visits",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            }
                        }
                },
                new ChartDataApexViewModel
                {
                    CampaignId = "2",
                    XAxis = new ApexXAxisViewModel
                    {
                        Type = "datetime",
                        Categories = new List<string>
                            {
                                "01/08/2022",
                                "01/09/2022",
                                "01/10/2022",
                                "01/11/2022",
                                "01/12/2022",
                                "01/13/2022",
                                "01/14/2022"
                            }
                    },
                    Series = new List<ApexAxisChartSeriesViewModel>
                        {
                            new()
                            {
                                Name = "Connections Sent",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Connections Accepted",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Replies",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Profile Visits",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                        }
                },
                new ChartDataApexViewModel
                {
                    CampaignId = "3",
                    XAxis = new ApexXAxisViewModel
                    {
                        Type = "datetime",
                        Categories = new List<string>
                            {
                                "01/15/2022",
                                "01/16/2022",
                                "01/17/2022",
                                "01/18/2022",
                                "01/19/2022",
                                "01/20/2022",
                                "01/21/2022"
                            }
                    },
                    Series = new List<ApexAxisChartSeriesViewModel>
                        {
                            new()
                            {
                                Name = "Connections Sent",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Connections Accepted",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Replies",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Profile Visits",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                        }
                },
                new ChartDataApexViewModel
                {
                    CampaignId = "4",
                    XAxis = new ApexXAxisViewModel
                    {
                        Type = "datetime",
                        Categories = new List<string>
                            {
                                "01/22/2022",
                                "01/23/2022",
                                "01/24/2022",
                                "01/25/2022",
                                "01/26/2022",
                                "01/27/2022",
                                "01/28/2022"
                            }
                    },
                    Series = new List<ApexAxisChartSeriesViewModel>
                        {
                            new()
                            {
                                Name = "Connections Sent",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Connections Accepted",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Replies",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                            new()
                            {
                                Name = "Profile Visits",
                                Data = new List<object>
                                {
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250),
                                    rnd.Next(250)
                                }
                            },
                        }
                }
            },
            DateFilters = new DateRangeViewModel
            {
                PrepopulatedFilters = new Dictionary<string, long>
                {
                    { "Today", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    { "Yesterday", DateTimeOffset.Now.AddDays(-1).ToUnixTimeSeconds() },
                    { "Last 3 Days", DateTimeOffset.Now.AddDays(-3).ToUnixTimeSeconds() },
                    { "Last 7 Days", DateTimeOffset.Now.AddDays(-7).ToUnixTimeSeconds() }
                }
            }
        };
    }
}
