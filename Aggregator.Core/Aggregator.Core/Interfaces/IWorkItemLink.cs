namespace Aggregator.Core.Interfaces
{
    public interface IWorkItemLink
    {
        string LinkTypeEndImmutableName { get; }

        int TargetId { get; }

        IWorkItem Target { get; }

        bool IsNew { get; }
    }
}
