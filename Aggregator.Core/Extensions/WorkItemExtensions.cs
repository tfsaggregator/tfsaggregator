using System.Linq;
using System.Text;

using Aggregator.Core.Interfaces;

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
                foreach (string s in wi.Validate().Cast<string>())
                {
                    sb.AppendLine(s);
                }
            }

            return sb.ToString();
        }
    }
}
