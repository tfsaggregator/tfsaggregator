namespace Aggregator.Core.Interfaces
{
    public interface IRevision
    {
        object this[string name] { get; }

        IFieldCollection Fields { get; }

        int Index { get; }

        IWorkItemLinkExposedCollection WorkItemLinks { get; }
    }
}