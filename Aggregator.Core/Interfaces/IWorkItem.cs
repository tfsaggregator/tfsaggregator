using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    using Microsoft.VisualStudio.Services.Profile;

    /// <summary>
    /// This interface is visible to scripts.
    /// </summary>
    public interface IWorkItemExposed
    {
        IFieldCollectionWrapper Fields { get; }
        TType GetField<TType>(string fieldName, TType defaultValue);
        string History { get; set; }
        int Id { get; }
        bool IsValid();
        object this[string name] { get; set; }
        string TypeName { get; }

        bool HasParent();

        bool HasChildren();

        bool HasRelation(string relation);

        // navigation helpers
        IWorkItemExposed Parent { get; }
        IEnumerable<IWorkItemExposed> Children { get; }
        IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query);
        // state helpers; must be on interface to work on WorkItemLazyReference
        void TransitionToState(string state, string comment);
    }

    /// <summary>
    /// This interface extends <see cref="IWorkItemExposed"/> with any WorkItem feature used in TFS Aggregator
    /// but not exposed to scripts.
    /// </summary>
    public interface IWorkItem : IWorkItemExposed
    {
        bool IsDirty { get; }
        void PartialOpen();
        void Save();
        void TryOpen();
        ArrayList Validate();
        IWorkItemLinkCollection WorkItemLinks { get; }
        IWorkItemType Type { get; }
    }
}
