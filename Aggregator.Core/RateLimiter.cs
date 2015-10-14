using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aggregator.Core.Context;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core
{
    public class RateLimiter
    {
        private readonly TimeSpan interval = TimeSpan.FromSeconds(10);

        private readonly int changes = 1;

        private readonly bool enabled;

        public RateLimiter(IRuntimeContext context)
        {
            if (context.Settings?.RateLimit != null)
            {
                this.enabled = true;
                this.interval = context.Settings.RateLimit.Interval;
                this.changes = context.Settings.RateLimit.Changes;
            }
        }

        public bool ShouldLimit(WorkItem wi)
        {
            if (!this.enabled)
            {
                return false;
            }

            if (wi.IsNew)
            {
                return false;
            }

            if (!wi.IsDirty)
            {
                return false;
            }

            DateTime watermark = DateTime.UtcNow;
            DateTime previousChangedDate = ((DateTime)wi.Fields[CoreField.ChangedDate].OriginalValue).ToUniversalTime();

            bool isRecentChange = watermark - previousChangedDate < this.interval;

            return isRecentChange && this.ThereAreMoreRevisionsInPeriod(wi, watermark);
        }

        private bool ThereAreMoreRevisionsInPeriod(WorkItem wi, DateTime watermark)
        {
            int inSpan = 0;
            for (int i = wi.Revisions.Count - 1; i > 0; i--)
            {
                var changedDate = (DateTime)wi.Revisions[i].Fields[CoreField.ChangedDate].Value;
                if (watermark - changedDate.ToUniversalTime() < this.interval)
                {
                    if (++inSpan > this.changes)
                    {
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }

            return false;
        }
    }
}
