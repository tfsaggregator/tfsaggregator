namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemLinkExposed
    {
        string LinkTypeEndImmutableName { get; }

        int TargetId { get; }

        IWorkItemExposed Target { get; }
    }
}