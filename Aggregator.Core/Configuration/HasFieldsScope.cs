using System;
using System.Linq;

using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Configuration
{
    public class HasFieldsScope : RuleScope
    {
        public string[] FieldNames { private get; set; }

        public override string DisplayName
        {
            get
            {
                return string.Format("Fields({0})", string.Join(", ", this.FieldNames));
            }
        }

        public override ScopeMatchResult Matches(IWorkItem item, INotification notification)
        {
            var res = new ScopeMatchResult();

            var trigger = this.FieldNames;

            var fields = item.Fields.ToArray();
            var available = fields.Select(f => f.Name).Concat(fields.Select(f => f.ReferenceName));

            res.AddRange(available);
            res.Success = trigger.All(t => available.Contains(t, StringComparer.OrdinalIgnoreCase));

            return res;
        }
    }
}