namespace Aggregator.Core
{
    public interface IWorkItemLink
    {
        string LinkTypeEndImmutableName { get; }
        int TargetId { get; }
        IWorkItem Target { get; }
    }
}
