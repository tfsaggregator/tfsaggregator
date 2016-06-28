using System;
using System.Linq;
using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Extensions
{
    public static class IWorkItemLinkTypeExtensions
    {
        public static bool IsForward(this IWorkItemLinkType workItemLinkType, string linkTypeName)
        {
            return new string[] { workItemLinkType.ForwardEndImmutableName, workItemLinkType.ForwardEndName }
                    .Contains(linkTypeName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
