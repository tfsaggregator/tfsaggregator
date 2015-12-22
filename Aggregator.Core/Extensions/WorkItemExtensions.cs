using System.Linq;
using System.Text;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Extensions
{
    public static class WorkItemExtensions
    {
        public static string GetInvalidWorkItemFieldsList(this IWorkItem wi)
        {
            StringBuilder sb = new StringBuilder();
            if (wi.IsValid())
            {
                sb.Append("None");
            }
            else
            {
                foreach (Field s in wi.Validate().Cast<Field>())
                {
                    sb.AppendLine(s.ReferenceName);
                }
            }

            return sb.ToString();
        }
    }
}
