namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemImplementation
    {
        IWorkItemLinkCollection WorkItemLinks { get; }
    }
}