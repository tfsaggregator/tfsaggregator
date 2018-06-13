using System;
using System.Collections.Generic;
using System.Linq;

using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Monitoring;

namespace Aggregator.Core
{
    /// <summary>
    /// Common code with Mocks
    /// </summary>
    public abstract class WorkItemImplementationBase : IWorkItemImplementation
    {
        /// <summary>
        /// Parent relation in LinkTypeEndImmutableName format.
        /// </summary>
        public const string ParentRelationship = "System.LinkTypes.Hierarchy-Reverse";

        /// <summary>
        /// Child relation in LinkTypeEndImmutableName format.
        /// </summary>
        public const string ChildRelationship = "System.LinkTypes.Hierarchy-Forward";

        protected ILogEvents Logger { get; }

        protected IWorkItemRepository Store { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkItemImplementationBase"/> class.
        /// </summary>
        protected WorkItemImplementationBase(IRuntimeContext context)
        {
            this.Store = context.WorkItemRepository;
            this.Logger = context.Logger;
        }

        /// <summary>
        /// Provides access to the WorkItemLinks collection of the underlying work item.
        /// </summary>
        /// <value>The WorkItemLinkCollection</value>
        public abstract IWorkItemLinkCollection WorkItemLinksImpl { get; }

        /// <summary>Checks whether this workitem has a relation of the specified type.</summary>
        /// <exception cref="ArgumentNullException"><paramref name="relation"/> is <see langword="null" />.</exception>
        /// <returns>true if this work item has a link of the specified type t another workitem.</returns>
        /// <param name="relation">The relation from this work item to another (case insensitive)</param>
        public bool HasRelation(string relation)
        {
            if (string.IsNullOrWhiteSpace(relation))
            {
                throw new ArgumentNullException(nameof(relation));
            }

            foreach (var link in this.WorkItemLinksImpl)
            {
                if (string.Equals(relation, link.LinkTypeEndImmutableName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the Parent work item (if any) or <see langword="null"/>.
        /// </summary>
        /// <value>Returns the Parent work item (if any) or <see langword="null"/>.</value>
        public IWorkItemExposed Parent
        {
            get
            {
                int? parentWorkItemId = (
                    from IWorkItemLink workItemLink in this.WorkItemLinksImpl
                    where workItemLink.LinkTypeEndImmutableName == ParentRelationship
                    select workItemLink.TargetId)
                    .Cast<int?>()
                    .FirstOrDefault();

                if (parentWorkItemId.HasValue)
                {
                    return this.Store.GetWorkItem(parentWorkItemId.Value);
                }

                return null;
            }
        }

        /// <summary>
        /// Returns all child work items
        /// </summary>
        /// <value>
        /// Returns all child work items
        /// </value>
        public IEnumerable<IWorkItemExposed> Children
        {
            get
            {
                var childWorkItems =
                    from IWorkItemLink workItemLink in this.WorkItemLinksImpl
                    where workItemLink.LinkTypeEndImmutableName == ChildRelationship
                    select this.Store.GetWorkItem(workItemLink.TargetId);

                foreach (var item in childWorkItems)
                {
                    yield return item;
                }
            }
        }
    }
}
