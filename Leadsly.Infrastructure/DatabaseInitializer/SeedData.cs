using Leadsly.Application.Model.ViewModels;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.Models.Entities.Campaigns.Phases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Campaign = Leadsly.Domain.Models.Entities.Campaigns.Campaign;
using CampaignProspect = Leadsly.Domain.Models.Entities.Campaigns.CampaignProspect;
using FollowUpMessage = Leadsly.Domain.Models.Entities.Campaigns.FollowUpMessage;
using PrimaryProspect = Leadsly.Domain.Models.Entities.Campaigns.PrimaryProspect;
using PrimaryProspectList = Leadsly.Domain.Models.Entities.Campaigns.PrimaryProspectList;
using SearchUrl = Leadsly.Domain.Models.Entities.Campaigns.SearchUrl;
using SearchUrlDetails = Leadsly.Domain.Models.Entities.Campaigns.SearchUrlDetails;
using SearchUrlProgress = Leadsly.Domain.Models.Entities.Campaigns.SearchUrlProgress;
using SendConnectionsStage = Leadsly.Domain.Models.Entities.Campaigns.SendConnectionsStage;

namespace Leadsly.Infrastructure.DatabaseInitializer
{
    public class SeedData
    {
        public static void Populate(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            DatabaseContext dbContext = serviceProvider.GetService<DatabaseContext>();

            Seed(dbContext, logger);
        }

        private static IList<string> InitialTimeZoneIds = new List<string>()
        {
            "Eastern Standard Time",
            "Central Standard Time",
            "Mountain Standard Time",
            "Pacific Standard Time",
            "Alaskan Standard Time",
            "Hawaiian Standard Time"
        };

        private static void Seed(DatabaseContext dbContext, ILogger<DatabaseInitializer> logger)
        {
            // populate database with UnitedStates timezones
            if (dbContext.SupportedTimeZones.Count() == 0)
            {
                foreach (string tzId in InitialTimeZoneIds)
                {
                    LeadslyTimeZone tz = new LeadslyTimeZone
                    {
                        TimeZoneId = tzId
                    };
                    dbContext.SupportedTimeZones.Add(tz);
                    dbContext.SaveChanges();
                }
            }

            if (dbContext.Campaigns.Count() == 0)
            {
                Campaign newCampaign = new()
                {
                    CampaignId = "99",
                    Name = "Testing Campaign",
                    StartTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    EndTimestamp = DateTimeOffset.Now.AddMonths(10).ToUnixTimeSeconds(),
                    DailyInvites = 3,
                    IsWarmUpEnabled = false,
                    CreatedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    CampaignType = CampaignTypeEnum.FollowUp,
                    FollowUpMessages = new List<FollowUpMessage>(),
                    ApplicationUserId = "1",
                    HalId = "1"
                };

                IList<string> searchUrls = new[] { "https://www.linkedin.com/search/results/people/?heroEntityKey=urn%3Ali%3Aautocomplete%3A692215045&keywords=gardners&origin=SWITCH_SEARCH_VERTICAL&position=0&searchId=41a6b0e0-e4aa-4017-8768-5d154cf444b5&sid=3Q1", "https://www.linkedin.com/search/results/people/?keywords=lawyer&origin=GLOBAL_SEARCH_HEADER&sid=Z0a" };
                string prospectListName = "test";

                PrimaryProspectList primaryProspectList = new()
                {
                    Name = prospectListName,
                    SearchUrls = new List<SearchUrl>(),
                    UserId = "1",
                    CreatedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                };

                foreach (string searchUrl in searchUrls)
                {
                    primaryProspectList.SearchUrls.Add(new()
                    {
                        PrimaryProspectList = primaryProspectList,
                        Url = searchUrl
                    });
                }

                dbContext.PrimaryProspectLists.Add(primaryProspectList);
                dbContext.SaveChanges();

                newCampaign.CampaignProspectList = new()
                {
                    PrimaryProspectList = primaryProspectList,
                    SearchUrls = primaryProspectList.SearchUrls.ToList(),
                    ProspectListName = primaryProspectList.Name,
                };

                IList<FollowUpMessage> followUpMsgs = new List<FollowUpMessage>();
                string contentA = "Hey thanks for connecting.";
                string contentB = "How are things going?";
                for (int i = 0; i < 2; i++)
                {
                    FollowUpMessage followUpMessage = default;
                    if (i == 0)
                    {
                        followUpMessage = new()
                        {
                            Campaign = newCampaign,
                            Content = contentA,
                            Order = 1,
                            Delay = new()
                            {
                                Unit = "mintues",
                                Value = 15
                            }
                        };
                    }

                    if (i == 1)
                    {
                        followUpMessage = new()
                        {
                            Campaign = newCampaign,
                            Content = contentB,
                            Order = 2,
                            Delay = new()
                            {
                                Unit = "days",
                                Value = 1
                            }
                        };
                    }

                    followUpMsgs.Add(followUpMessage);
                }

                newCampaign.FollowUpMessages = followUpMsgs;
                newCampaign.ProspectListPhase = new ProspectListPhase
                {
                    Campaign = newCampaign,
                    Completed = false,
                    PhaseType = PhaseType.ProspectList,
                    SearchUrls = searchUrls.ToList()
                };
                newCampaign.FollowUpMessagePhase = new()
                {
                    Campaign = newCampaign,
                    PhaseType = PhaseType.FollwUpMessage
                };

                IList<SearchUrlDetails> sentConnectionsSearchUrlStatuses = new List<SearchUrlDetails>();
                foreach (string searchUrl in searchUrls)
                {
                    SearchUrlDetails sentConnectionsSearchUrlStatus = new()
                    {
                        Campaign = newCampaign,
                        CurrentUrl = searchUrl,
                        FinishedCrawling = false,
                        StartedCrawling = false,
                        OriginalUrl = searchUrl,
                        WindowHandleId = string.Empty,
                        LastActivityTimestamp = 0
                    };
                    sentConnectionsSearchUrlStatuses.Add(sentConnectionsSearchUrlStatus);
                }
                newCampaign.SentConnectionsStatuses = sentConnectionsSearchUrlStatuses;

                IList<SearchUrlProgress> searchUrlsProgress = new List<SearchUrlProgress>();
                foreach (string searchUrl in searchUrls)
                {
                    SearchUrlProgress searchUrlProgress = new()
                    {
                        Campaign = newCampaign,
                        LastPage = 1,
                        LastProcessedProspect = 0,
                        TotalSearchResults = 0,
                        SearchUrl = searchUrl,
                        WindowHandleId = string.Empty
                    };
                    searchUrlsProgress.Add(searchUrlProgress);
                }
                newCampaign.SearchUrlsProgress = searchUrlsProgress;

                IList<SendConnectionsStage> connectionsStages = new List<SendConnectionsStage>();
                for (int i = 0; i < 3; i++)
                {
                    SendConnectionsStage sendConnectionsStage = new();
                    if (i == 0)
                    {
                        sendConnectionsStage = new SendConnectionsStage
                        {
                            StartTime = "8:00 AM",
                            Order = i + 1
                        };
                    }
                    else if (i == 1)
                    {
                        sendConnectionsStage = new SendConnectionsStage
                        {
                            StartTime = "12:00 PM",
                            Order = i + 1
                        };
                    }
                    else
                    {
                        sendConnectionsStage = new SendConnectionsStage
                        {
                            StartTime = "5:00PM",
                            Order = i + 1
                        };
                    }

                    connectionsStages.Add(sendConnectionsStage);
                }
                newCampaign.SendConnectionStages = connectionsStages;

                dbContext.Campaigns.Add(newCampaign);
                dbContext.SaveChanges();

                IList<CampaignProspect> campaignProspects = new List<CampaignProspect>();
                IList<PrimaryProspect> prospects = new List<PrimaryProspect>();
                PrimaryProspect oskar = new()
                {
                    AddedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = "Oskar Mikolajczyk",
                    Area = "Cleveland",
                    EmploymentInfo = "Progressive Insurance",
                    PrimaryProspectListId = primaryProspectList.PrimaryProspectListId,
                    ProfileUrl = "https://www.linkedin.com/in/oskar-mikolajczyk/",
                    SearchResultAvatarUrl = "",
                    PrimaryProspectList = primaryProspectList
                };
                prospects.Add(oskar);

                PrimaryProspect malyssa = new()
                {
                    AddedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = "Malyssa Doyle",
                    Area = "Cleveland",
                    EmploymentInfo = "Software Development Engineer II at Clario",
                    PrimaryProspectListId = primaryProspectList.PrimaryProspectListId,
                    ProfileUrl = "https://www.linkedin.com/in/malyssadoyle/",
                    SearchResultAvatarUrl = "",
                    PrimaryProspectList = primaryProspectList
                };

                prospects.Add(malyssa);

                PrimaryProspect slick = new()
                {
                    AddedTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    Name = "Slick Reddington",
                    Area = "Cleveland",
                    EmploymentInfo = "Software Development Engineer II at Clario",
                    PrimaryProspectListId = primaryProspectList.PrimaryProspectListId,
                    ProfileUrl = "https://www.linkedin.com/in/slick-reddington-0a41a7237/",
                    SearchResultAvatarUrl = "",
                    PrimaryProspectList = primaryProspectList
                };

                prospects.Add(slick);

                dbContext.PrimaryProspects.AddRange(prospects);
                dbContext.SaveChanges();

                CampaignProspect oskarCampaign = new()
                {
                    PrimaryProspect = oskar,
                    Accepted = true,
                    CampaignId = newCampaign.CampaignId,
                    CampaignProspectListId = newCampaign.CampaignProspectList.CampaignProspectListId,
                    Name = "Oskar Mikolajczyk",
                    ProfileUrl = "https://www.linkedin.com/in/oskar-mikolajczyk/",
                    ConnectionSent = false,
                    ConnectionSentTimestamp = 0,
                    FollowUpMessageSent = false,
                    Campaign = newCampaign,
                    LastFollowUpMessageSentTimestamp = 0,
                    FollowUpComplete = false,
                    CampaignProspectList = newCampaign.CampaignProspectList,
                    Replied = false,
                    PrimaryProspectId = oskar.PrimaryProspectId
                };
                campaignProspects.Add(oskarCampaign);

                CampaignProspect malyssaCampaign = new()
                {
                    PrimaryProspect = malyssa,
                    Accepted = true,
                    CampaignId = newCampaign.CampaignId,
                    CampaignProspectListId = newCampaign.CampaignProspectList.CampaignProspectListId,
                    Name = "Malyssa Doyle",
                    ProfileUrl = "https://www.linkedin.com/in/malyssadoyle/",
                    ConnectionSent = true,
                    ConnectionSentTimestamp = 1234568,
                    FollowUpMessageSent = false,
                    Campaign = newCampaign,
                    LastFollowUpMessageSentTimestamp = 12333654,
                    FollowUpComplete = false,
                    CampaignProspectList = newCampaign.CampaignProspectList,
                    Replied = false,
                    PrimaryProspectId = malyssa.PrimaryProspectId
                };
                campaignProspects.Add(malyssaCampaign);

                CampaignProspect slickCampaign = new()
                {
                    PrimaryProspect = malyssa,
                    Accepted = true,
                    CampaignId = newCampaign.CampaignId,
                    CampaignProspectListId = newCampaign.CampaignProspectList.CampaignProspectListId,
                    Name = "Slick Reddington",
                    ProfileUrl = "https://www.linkedin.com/in/malyssadoyle/",
                    ConnectionSent = true,
                    ConnectionSentTimestamp = 1234568,
                    FollowUpMessageSent = true,
                    LastFollowUpMessageSentTimestamp = 12333654,
                    AcceptedTimestamp = 123444,
                    Campaign = newCampaign,
                    SentFollowUpMessageOrderNum = 1,
                    FollowUpComplete = false,
                    CampaignProspectList = newCampaign.CampaignProspectList,
                    Replied = false,
                    PrimaryProspectId = slick.PrimaryProspectId

                };
                campaignProspects.Add(slickCampaign);

                dbContext.CampaignProspects.AddRange(campaignProspects);
                dbContext.SaveChanges();

            }

        }
    }
}
