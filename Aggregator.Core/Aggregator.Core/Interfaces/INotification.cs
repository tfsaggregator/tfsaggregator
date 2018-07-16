using System.Collections.Generic;

namespace Aggregator.Core.Interfaces
{
    // Identical to Microsoft.TeamFoundation.WorkItemTracking.Server.ChangeTypes
    public enum ChangeTypes
    {
        New = 0,
        Change = 1,
        Delete = 2,
        Restore = 3
    }

    // Identical to Microsoft.TeamFoundation.WorkItemTracking.Server.AddedResourceLink
    public class AddedResourceLink
    {
        public string Resource { get; set; }

        public static implicit operator AddedResourceLink(Microsoft.TeamFoundation.WorkItemTracking.Server.AddedResourceLink addedResourceLink)
        {
            return new AddedResourceLink { Resource = addedResourceLink.Resource };
        }
    }

    /// <summary>
    /// Decouples Core from TFS Server API
    /// </summary>
    public interface INotification
    {
        string ProjectUri
        {
            get;
        }

        ChangeTypes ChangeType
        {
            get;
        }

        int WorkItemId
        {
            get;
        }

        string ChangerTeamFoundationId
        {
            get;
        }

        IEnumerable<Interfaces.AddedResourceLink> AddedResourceLinks { get; }
    }
}
