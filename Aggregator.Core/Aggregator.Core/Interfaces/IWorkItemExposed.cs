using System;
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

        IRevision PreviousRevision
        {
            get;
        }

        IRevision NextRevision
        {
            get;
        }

        bool IsValid();

        object this[string name] { get; set; }

        string TypeName { get; }

        int Id { get; }

        Uri Uri { get; }

        // navigation helpers
        bool HasRelation(string relation);

        IWorkItemExposed Parent { get; }

        IEnumerable<IWorkItemExposed> Children { get; }

        IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query);

        IEnumerable<Interfaces.AddedResourceLink> AddedResourceLinks { get; }

        // links management
        IWorkItemLinkExposedCollection WorkItemLinks { get; }

        IReadOnlyList<ExternalLink> ExternalLinks { get; }

        void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName);

        void RemoveWorkItemLink(IWorkItemLinkExposed link);

        void AddHyperlink(string destination);

        void AddHyperlink(string destination, string message);

        void AddChangesetLink(string changeSetUri, string comment);

        void AddSourceFileLink(string sourceFileUri, string comment);

        // state helpers; must be on interface to work on WorkItemLazyReference
        void TransitionToState(string state, string comment);
    }
}