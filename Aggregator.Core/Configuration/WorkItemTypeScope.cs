using System;
using System.Linq;

using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    public class WorkItemTypeScope : RuleScope
    {
        public string[] ApplicableTypes { private get; set; }

        public override bool Matches(IWorkItem item)
        {
            return this.ApplicableTypes.Contains(item.TypeName, StringComparer.OrdinalIgnoreCase);
        }
    }
}