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
    }
}
