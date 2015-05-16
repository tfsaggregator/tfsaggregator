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

        // fluent API for GetRelatives
        public static FluentQuery WhereTypeIs(this IWorkItem wi, string workItemType)
        {
            return new FluentQuery(wi).WhereTypeIs(workItemType);
        }

        public static FluentQuery AtMost(this IWorkItem wi, int levels)
        {
            return new FluentQuery(wi).AtMost(levels);
        }

        public static FluentQuery FollowingLinks(this IWorkItem wi, string linkType)
        {
            return new FluentQuery(wi).FollowingLinks(linkType);
        }
    }
}
