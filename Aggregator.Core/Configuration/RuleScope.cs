using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Configuration
{
    using Microsoft.TeamFoundation.Framework.Client.Catalog.Objects;

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
            var fields = item.Fields.ToArray();

            return
                this.FieldNames.All(
                    fn =>
                        fields.Any(
                            f =>
                                string.Equals(fn, f.Name, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(fn, f.ReferenceName, StringComparison.OrdinalIgnoreCase)));
        }
    }

}
