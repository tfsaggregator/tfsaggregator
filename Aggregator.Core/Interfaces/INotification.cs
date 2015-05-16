namespace Aggregator.Core
{
    /// <summary>
    /// Decouples Core from TFS Server API
    /// </summary>
    public interface INotification
    {
        string ProjectUri { get; }
        int WorkItemId { get; }
    }
}
