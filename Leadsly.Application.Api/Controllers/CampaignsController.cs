using Leadsly.Domain.Models;
using Leadsly.Domain.Supervisor;
using Leadsly.Application.Model.ViewModels.Campaigns;
using Leadsly.Application.Model.ViewModels.Reports;
using Leadsly.Application.Model.ViewModels.Reports.ApexCharts;
using Leadsly.Application.Model.Entities;
using Leadsly.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Leadsly.Application.Model.ViewModels.Response.Hal;
using System.Security.Claims;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;

namespace Leadsly.Application.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CampaignsController : ApiControllerBase
    {
        protected static List<CampaignViewModel> Campaigns { get; set; } = new()
        {
            new CampaignViewModel
            {
                Id = "1",
                Active = true,
                Name = "Lawyers",
                ConnectionsSentDaily = 60,
                ConnectionsAccepted = 231,
                TotalConnectionsSent = 443,
                ProfileViews = 192,
                Replies = 136,
                Expired = false,
                Notes = "this is my note about this campaign because it was very successful and i love this app. WOW how could anyone have had such a great idea."
            },
            new CampaignViewModel
            {
                Id = "2",
                Active = true,
                Name = "FBI Agents",
                ConnectionsSentDaily = 50,
                ConnectionsAccepted = 351,
                TotalConnectionsSent = 143,
                ProfileViews = 192,
                Replies = 636,
                Expired = false,
                Notes = ""
            },
            new CampaignViewModel
            {
                Id = "3",
                Active = true,
                Name = "Real Estate",
                ConnectionsSentDaily = 60,
                ConnectionsAccepted = 231,
                TotalConnectionsSent = 443,
                ProfileViews = 192,
                Replies = 136,
                Expired = false,
                Notes = "this is my note about this campaign because it was very successful and i love this app. WOW how could anyone have had such a great idea."
            },
            new CampaignViewModel
            {
                Id = "4",
                Active = true,
                Name = "Rich Women",
                ConnectionsSentDaily = 60,
                ConnectionsAccepted = 231,
                TotalConnectionsSent = 443,
                ProfileViews = 192,
                Replies = 136,
                Expired = false,
                Notes = "this is my note about this campaign because it was very successful and i love this app. WOW how could anyone have had such a great idea."
            },
            new CampaignViewModel
            {
                Id = "5",
                Active = true,
                Name = "Mushroom Lovers",
                ConnectionsSentDaily = 60,
                ProfileViews = 192,
                ConnectionsAccepted = 231,
                TotalConnectionsSent = 443,
                Replies = 136,
                Expired = false,
                Notes = "this is my note about this campaign because it was very successful and i love this app. WOW how could anyone have had such a great idea."
            }
        };

        private static Random rnd = new Random();

        protected static CampaignsReportViewModel CampaignsEffectivenessReports { get; set; } = new CampaignsReportViewModel
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
        public CampaignsController(ILogger<CampaignsController> logger, ISupervisor supervisor)
        {
            _logger = logger;
            _supervisor = supervisor;
        }

        private readonly ISupervisor _supervisor;
        private readonly ILogger<CampaignsController> _logger;

        [HttpGet("{id}")]
        public IActionResult GetCampaign(string id)
        {
            CampaignViewModel campaign = Campaigns.Find(c => c.Id == id);

            if(id == null)
            {
                BadRequest_CampaignNotFound();
            }

            return Ok(campaign);
        }

        [HttpPut]
        public IActionResult UpdateCampaign([FromBody] CampaignViewModel updateCampaign, CancellationToken ct = default)
        {
            CampaignViewModel staleCampaign = Campaigns.Find((c) => c.Id == updateCampaign.Id);
            int index = Campaigns.IndexOf(staleCampaign);
            Campaigns.RemoveAt(index);
            Campaigns.Insert(index, updateCampaign);            

            return new JsonResult(updateCampaign);
        }

        [HttpPatch]
        public IActionResult ToggleActiveCampaign([FromBody] ToggleCampaignViewModel toggleCampaign, CancellationToken ct = default)
        {
            CampaignViewModel toggleCampaignStatus = Campaigns.Find((c) => c.Id == toggleCampaign.Id);
            toggleCampaignStatus.Active = !toggleCampaignStatus.Active;

            return new JsonResult(toggleCampaignStatus);
        }

        [HttpPost(":id/prospects")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateContactedCampaignProspects(string campaignId, [FromBody] CampaignProspectListRequest request, CancellationToken ct = default)
        {
            HalOperationResult<IOperationResponse> result = await _supervisor.ProcessConnectionRequestSentForCampaignProspectsAsync<IOperationResponse>(request, ct);

            return Ok();
        }

        //[HttpGet("reports/effectiveness")]
        //public IActionResult CampaignEffectivenessReports([FromQuery] List<string> ids, [FromQuery] long filterCriteria = 0L, [FromQuery] long startDate = 0L, [FromQuery] long endDate = 0L)
        //{
        //    List<CampaignViewModel> filteredCampaigns = Campaigns.Where(c => ids.Contains(c.Id)).ToList();

        //    if(filteredCampaigns == null)
        //    {
        //        // To Do return custom bad request
        //        return BadRequest();
        //    }

        //    if(filterCriteria != 0L)
        //    {
        //        DateTimeOffset
        //    }
        //}

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(CreateCampaignRequest request, CancellationToken ct = default)
        {
            string userId = "ea435588-489f-444c-8f9d-01a4988750b9"; // User.FindFirst(ClaimTypes.NameIdentifier)?.Value; ;            
            var a = await _supervisor.CreateCampaignAsync<ConnectResponseViewModel>(request, userId, ct);
            return Ok();
        }

        [HttpPost("{id}/clone")]
        public IActionResult CloneCampaign([FromBody] CloneCampaignViewModel cloneCampaign, CancellationToken ct = default)
        {
            CampaignViewModel campaignToClone = Campaigns.Find((c) => c.Id == cloneCampaign.Id);

            // you must figure out a good way of performing deep cloning of objects.
            CampaignViewModel clonedCampaign = new CampaignViewModel
            {
                Id = Guid.NewGuid().ToString(),
                Active = false,
                ConnectionsAccepted = campaignToClone.ConnectionsAccepted,
                ConnectionsSentDaily = campaignToClone.ConnectionsSentDaily,
                Expired = campaignToClone.Expired,
                Name = campaignToClone.Name,
                Notes = campaignToClone.Notes,
                ProfileViews = campaignToClone.ProfileViews,
                Replies = campaignToClone.Replies,
                TotalConnectionsSent = campaignToClone.TotalConnectionsSent
            };

            Campaigns.Add(clonedCampaign);

            return new JsonResult(clonedCampaign);
        }

        [HttpDelete]
        public IActionResult DeleteCampaign([FromBody] DeleteCampaignViewModel deleteCampaign, CancellationToken ct = default)
        {
            CampaignViewModel delete = Campaigns.Find((c) => c.Id == deleteCampaign.Id);
            Campaigns.Remove(delete);

            return NoContent();
        }

    }
}
