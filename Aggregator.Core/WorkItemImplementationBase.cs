using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core
{
    /// <summary>
    /// Common code with Mocks
    /// </summary>
    public abstract class WorkItemImplementationBase : IWorkItemImplementation
    {
        public static readonly string ParentRelationship = "System.LinkTypes.Hierarchy-Reverse";
        public static readonly string ChildRelationship = "System.LinkTypes.Hierarchy-Forward";

        protected ILogEvents logger;
        protected IWorkItemRepository store;
        protected List<Lazy<IWorkItem>> childrenWorkItems;

        public WorkItemImplementationBase(IWorkItemRepository store, ILogEvents logger)
        {
            this.store = store;
            this.logger = logger;
        }

        public abstract IWorkItemLinkCollection WorkItemLinks { get; }

        public bool HasRelation(string relation)
        {
            if (string.IsNullOrWhiteSpace(relation))
            {
                throw new ArgumentNullException("relation");
            }//if

            foreach (var link in this.WorkItemLinks)
            {
                if (string.Equals(relation, link.LinkTypeEndImmutableName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }//if
            }//for

            return false;
        }

        public IWorkItemExposed Parent
        {
            get
            {
                int parentWorkItemId = (from IWorkItemLink workItemLink in this.WorkItemLinks
                                        where workItemLink.LinkTypeEndImmutableName == ParentRelationship
                                        select workItemLink.TargetId).FirstOrDefault();
                return this.store.GetWorkItem(parentWorkItemId);
            }
        }

        public IEnumerable<IWorkItemExposed> Children
        {
            get
            {
                var childrenWorkItems = from IWorkItemLink workItemLink in this.WorkItemLinks
                                        where workItemLink.LinkTypeEndImmutableName == ChildRelationship
                                        select this.store.GetWorkItem(workItemLink.TargetId);
                foreach (var item in childrenWorkItems)
                {
                    yield return item;
                }//for
            }
        }
    }
}
