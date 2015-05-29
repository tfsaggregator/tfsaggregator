namespace Aggregator.Core
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// This interface is visible to scripts.
    /// </summary>
    public interface IWorkItemExposed
    {
        IFieldCollectionWrapper Fields { get; }
        string History { get; set; }
        bool IsValid();
        object this[string name] { get; set; }
        string TypeName { get; }
        int Id { get; }

        // navigation helpers
        bool HasRelation(string relation);
        IWorkItemExposed Parent { get; }
        IEnumerable<IWorkItemExposed> Children { get; }
        IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query);
        // links management
        void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName);
        void AddHyperlink(string destination, string comment = "");
        // state helpers; must be on interface to work on WorkItemLazyReference
        void TransitionToState(string state, string comment);
    }

    // common implmementation
    public interface IWorkItemImplementation
    {
        IWorkItemLinkCollection WorkItemLinks { get; }
    }

    /// <summary>
    /// This interface extends <see cref="IWorkItemExposed"/> with any WorkItem feature used in TFS Aggregator
    /// but not exposed to scripts.
    /// </summary>
    public interface IWorkItem : IWorkItemExposed, IWorkItemImplementation
    {
        bool IsDirty { get; }
        void PartialOpen();
        void Save();
        void TryOpen();
        ArrayList Validate();
        IWorkItemType Type { get; }
    }
}
