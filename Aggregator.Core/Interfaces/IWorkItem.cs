using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core.Navigation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Interfaces
{
    /// <summary>
    /// This interface is visible to scripts.
    /// </summary>
    public interface IWorkItemExposed
    {
        IFieldCollection Fields { get; }

        string History { get; set; }

        DateTime RevisedDate { get; }

        int Revision { get; }

        IRevision LastRevision { get; }

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

        bool ShouldLimit(RateLimiter limiter);
    }
}
