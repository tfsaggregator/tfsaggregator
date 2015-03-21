using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
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
        // navigation helper
        IWorkItemExposed Parent { get; }
    }

    /// <summary>
    /// This interface extends <see cref="IWorkItemExposed"/> with any WorkItem feature used in TFS Aggregator
    /// but not exposed to scripts.
    /// </summary>
    public interface IWorkItem : IWorkItemExposed
    {
        void PartialOpen();
        void Save();
        void TryOpen();
        ArrayList Validate();
        WorkItemLinkCollection WorkItemLinks { get; }
    }
}
