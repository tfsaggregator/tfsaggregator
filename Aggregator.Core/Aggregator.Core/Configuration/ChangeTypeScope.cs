using System;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    public class ChangeTypeScope : RuleScope
    {
        public string[] ApplicableChanges { private get; set; }

        public override string DisplayName
        {
            get
            {
                return string.Format("ChangeTypes({0})", string.Join(", ", this.ApplicableChanges));
            }
        }

        public override ScopeMatchResult Matches(IWorkItem item, INotification notification)
        {
            var res = new ScopeMatchResult();
            res.Add(notification.ChangeType.ToString());
            res.Success = this.ApplicableChanges.Any(type => type.SameAs(notification.ChangeType.ToString()));
            return res;
        }
    }
}