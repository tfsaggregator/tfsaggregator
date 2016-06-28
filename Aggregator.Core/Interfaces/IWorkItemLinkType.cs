using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// Decouples Core from TFS Client API <see cref="Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemLinkType"/>
    /// </summary>
    public interface IWorkItemLinkType
    {
        string ForwardEndName { get; }

        string ForwardEndImmutableName { get; }

        string ReferenceName { get; }

        string ReverseEndName { get; }

        string ReverseEndImmutableName { get; }
    }
}