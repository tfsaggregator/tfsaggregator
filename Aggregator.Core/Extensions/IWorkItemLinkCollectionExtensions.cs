using System.Collections.Generic;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Extensions
{
    public static class IWorkItemLinkCollectionExtensions
    {
        public static IEnumerable<IWorkItemLink> WhereMatches(this IWorkItemLinkCollection linkColl, string linkType)
        {
            if (linkType == "*")
            {
                return linkColl;
            }
            else
            {
                return linkColl.Filter(linkType);
            }
        }
    }
}
