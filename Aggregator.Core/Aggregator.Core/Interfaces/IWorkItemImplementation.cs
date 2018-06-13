namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemImplementation
    {
        IWorkItemLinkCollection WorkItemLinksImpl { get; }
    }
}