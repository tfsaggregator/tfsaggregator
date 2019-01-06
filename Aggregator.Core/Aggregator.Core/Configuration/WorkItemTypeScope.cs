using System;
using System.Linq;

using Aggregator.Core.Extensions;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    public class WorkItemTypeScope : RuleScope
    {
        public string[] ApplicableTypes { private get; set; }

        public override string DisplayName
        {
            get
            {
                return string.Format("WorkItemTypes({0})", string.Join(", ", this.ApplicableTypes));
            }
        }

        public override ScopeMatchResult Matches(IWorkItem item, INotification notification)
        {
            var res = new ScopeMatchResult();
            res.Add(item.TypeName);
            res.Success = this.ApplicableTypes.Any(type => type.SameAs(item.TypeName));
            return res;
        }
    }
}