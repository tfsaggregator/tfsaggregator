namespace Aggregator.Core.Interfaces
{
    public interface IRevision
    {
        object this[string name] { get; }

        IFieldCollectionWrapper Fields { get; }

        int Index { get; }
    }
}