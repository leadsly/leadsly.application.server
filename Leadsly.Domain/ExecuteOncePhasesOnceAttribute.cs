using Hangfire.Client;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Leadsly.Domain
{
    public class ExecuteOncePhasesOnceAttribute : JobFilterAttribute, IClientFilter, IApplyStateFilter
    {
        public void OnCreating(CreatingContext filterContext)
        {
            var entries = filterContext.Connection.GetAllEntriesFromHash(GetJobKey(filterContext.Job));
            if (entries != null && entries.ContainsKey("jobId"))
            {
                // this job was already created once, cancel creation
                filterContext.Canceled = true;
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {
            if (!filterContext.Canceled)
            {
                // job created, mark it as such
                filterContext.Connection.SetRangeInHash(GetJobKey(filterContext.Job),
                new[] { new KeyValuePair<string, string>("jobId", filterContext.BackgroundJob?.Id) });
            }
        }

        public void OnStateApplied(ApplyStateContext filterContext, IWriteOnlyTransaction transaction)
        {
            if (filterContext.NewState.Name.Equals(Hangfire.States.SucceededState.StateName)
                || filterContext.NewState.Name.Equals(Hangfire.States.FailedState.StateName))
            {
                RemoveFingerprintKey(filterContext.Connection, filterContext.BackgroundJob.Job);
            }
        }

        private static void RemoveFingerprintKey(IStorageConnection connection, Job job)
        {
            using (var transaction = connection.CreateWriteTransaction())
            {
                transaction.RemoveHash(GetJobKey(job));
                transaction.Commit();
            }
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {

        }

        private static string GetJobKey(Job job)
        {
            using (var sha256 = SHA256.Create())
            {
                return "exec-once:" + Convert.ToBase64String(
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(job.ToString() + job.Args[2])));
            }
        }
    }
}
