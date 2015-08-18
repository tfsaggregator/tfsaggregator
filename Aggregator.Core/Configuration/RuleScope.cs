using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    using System;
    using System.Linq;

    public abstract class RuleScope
    {
        public abstract bool Matches(IWorkItem item);
    }

    public class WorkItemTypeScope : RuleScope
    {
        public string[] ApplicableTypes { private get; set; }

        public override bool Matches(IWorkItem item)
        {
            return this.ApplicableTypes.Contains(item.TypeName, StringComparer.OrdinalIgnoreCase);
        }
    }

    public class HasFieldsScope : RuleScope
    {
        public string[] FieldNames { private get; set; }

        public override bool Matches(IWorkItem item)
        {
            var trigger = FieldNames;

            var fields = item.Fields.ToArray();
            var available = fields.Select(f => f.Name).Concat(fields.Select(f => f.ReferenceName));

            return trigger.All(t => available.Contains(t, StringComparer.OrdinalIgnoreCase));
        }
    }
}