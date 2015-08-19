using System;
using System.Linq;

using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    public class HasFieldsScope : RuleScope
    {
        public string[] FieldNames { private get; set; }

        public override bool Matches(IWorkItem item)
        {
            var trigger = this.FieldNames;

            var fields = item.Fields.ToArray();
            var available = fields.Select(f => f.Name).Concat(fields.Select(f => f.ReferenceName));

            return trigger.All(t => available.Contains(t, StringComparer.OrdinalIgnoreCase));
        }
    }
}