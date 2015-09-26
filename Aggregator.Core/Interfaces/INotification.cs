namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// Decouples Core from TFS Server API
    /// </summary>
    public interface INotification
    {
        string ProjectUri
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
