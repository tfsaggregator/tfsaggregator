namespace Aggregator.Core
{
    using System.Text;

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
                foreach (string s in wi.Validate())
                    sb.AppendLine(s);
            }
            return sb.ToString();
        }
    }
}
