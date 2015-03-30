using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aggregator.Core.Navigation
{
    public class WorkItemLazyVisitor : IEnumerable<IWorkItemExposed> 
    {
        // source info
        IWorkItemRepository store;
        IWorkItem sourceWorkItem;
        // target filter
        string workItemType;
        int levels;
        string linkType;

        public static WorkItemLazyVisitor MakeRelativesLazyVisitor(IWorkItem sourceItem, FluentQuery query, IWorkItemRepository store)
        {
            return new WorkItemLazyVisitor(sourceItem, query, store);
        }

        public WorkItemLazyVisitor(IWorkItem sourceWorkItem, FluentQuery query, IWorkItemRepository store)
        {
            if (query.Levels < 1)
                throw new ArgumentOutOfRangeException("levels must be 1 or greater");
            // source info
            this.sourceWorkItem = sourceWorkItem;
            this.store = store;
            // target filter
            this.workItemType = query.WorkItemType;
            this.levels = query.Levels;
            this.linkType = query.LinkType;
        }

        public IEnumerator<IWorkItemExposed> GetEnumerator()
        {
            var links = new List<Tuple<int, IWorkItemLink>>(
                this.sourceWorkItem.WorkItemLinks
                    .Where(link => link.LinkTypeEndImmutableName.SameAs(this.linkType))
                    .Select(link => Tuple.Create(this.levels - 1, link))
                    );

            while (links.Any())
            {
                var current = links.First();
                links.RemoveAt(0);

                IWorkItem relatedWorkItem = current.Item2.Target;
                if (relatedWorkItem.TypeName.SameAs(workItemType))
                {
                    yield return relatedWorkItem;
                }//if
                    if (current.Item1 > 0)
                    {
                        // add to end => depth-first
                        links.AddRange(
                            relatedWorkItem.WorkItemLinks
                            .Where(link => link.LinkTypeEndImmutableName.SameAs(this.linkType))
                            .Select(link => Tuple.Create(current.Item1 - 1, link))
                            );
                    }//if
            }//while
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
