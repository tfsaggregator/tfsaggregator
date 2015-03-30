using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aggregator.Core
{
    public class FluentQuery : IEnumerable<IWorkItemExposed>
    {
        IWorkItemExposed host;
        public FluentQuery(IWorkItemExposed host)
        {
            this.host = host;

            // defaults
            this.WorkItemType = "*";
            this.Levels = 1;
            this.LinkType = "*";
        }

        public string WorkItemType { get; set; }
        public int Levels { get; set; }
        public string LinkType { get; set; }

        public FluentQuery WhereTypeIs(string workItemType)
        {
            this.WorkItemType = workItemType;
            return this;
        }

        public FluentQuery AtMost(int levels)
        {
            this.Levels = levels;
            return this;
        }

        public FluentQuery FollowingLinks(string linkType)
        {
            this.LinkType = linkType;
            return this;
        }

        public IEnumerable<IWorkItemExposed> AsEnumerable()
        {
            return this.host.GetRelatives(this);
        }

        public IEnumerator<IWorkItemExposed> GetEnumerator()
        {
            return this.host.GetRelatives(this).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
