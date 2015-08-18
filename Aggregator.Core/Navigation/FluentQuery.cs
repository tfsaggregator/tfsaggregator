using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core.Interfaces;

namespace Aggregator.Core.Navigation
{
    public class FluentQuery : IEnumerable<IWorkItemExposed>
    {
        private readonly IWorkItemExposed host;

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

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (!(obj is FluentQuery))
            {
                return base.Equals(obj);
            }

            var rhs = obj as FluentQuery;

            return
                string.Equals(this.WorkItemType, rhs.WorkItemType, StringComparison.OrdinalIgnoreCase)
                && this.Levels == rhs.Levels
                && string.Equals(this.LinkType, rhs.LinkType, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
