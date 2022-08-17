using Hangfire;

namespace Leadsly.Application.Model
{
    public static class HangFireConstants
    {
        public static class RecurringJobs
        {
            public static string ScheduleRestartForNewTimeZones = "RestartHalNewTimeZones";
            public static string ScheduleNewTimeZones = "ScheduleNewTimeZones";
            public static string DailyCronSchedule = Cron.Daily(4, 40);
        }
    }
}
