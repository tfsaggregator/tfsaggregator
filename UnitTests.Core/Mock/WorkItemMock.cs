using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Navigation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Linq;

namespace UnitTests.Core.Mock
{
    internal class WorkItemMock : WorkItemImplementationBase, IWorkItem
    {
        private readonly FieldCollectionMock fields;
        private readonly WorkItemLinkCollectionMock workItemLinks;

        public WorkItemMock(IWorkItemRepository repository, IRuntimeContext context)
            : base(context)
        {
            this.fields = new FieldCollectionMock(this);
            this.workItemLinks = new WorkItemLinkCollectionMock(repository);
            this.IsDirty = false;
        }

        public IFieldCollection Fields
        {
            get
            {
                return this.fields;
            }
        }

        public string History { get; set; }

        public int Id { get; set; }

        public bool IsValid()
        {
            return true;
        }

        public bool IsDirty { get; set; }

        public Uri Uri { get; set; }

        public void PartialOpen()
        {
            // No functionality needed in mock.
        }

        private int internalSaveCalled = 0;

        public bool InternalWasSaveCalled
        {
            get
            {
                return this.internalSaveCalled > 0;
            }
        }

        public int InternalSaveCount
        {
            get
            {
                return this.internalSaveCalled;
            }
        }

        public void Save()
        {
            this.internalSaveCalled++;
        }

        public object this[string name]
        {
            get
            {
                return this.Fields[name].Value;
            }

            set
            {
                this.Fields[name].Value = value;
            }
        }

        public void TryOpen()
        {
            // No functionality needed in mock.
        }

        public string TypeName { get; set; }

        public ArrayList Validate()
        {
            return new ArrayList();
        }

        public override IWorkItemLinkCollection WorkItemLinks
        {
            get { return this.workItemLinks; }
        }

        public IWorkItemType Type { get; set; }

        public DateTime RevisedDate
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Revision
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IRevision LastRevision
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IRevision PreviousRevision
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IRevision NextRevision
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool ShouldLimit(RateLimiter limiter)
        {
            return false;
        }

        public IEnumerable<IWorkItemExposed> GetRelatives(FluentQuery query)
        {
            return WorkItemLazyVisitor
                .MakeRelativesLazyVisitor(this, query);
        }

        public void TransitionToState(string state, string comment)
        {
            StateWorkFlow.TransitionToState(this, state, comment, this.Logger);
        }

        public void AddWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            bool anyChange = this.DoAddWorkItemLink(destination, linkTypeName);
            this.IsDirty = anyChange;
            ((WorkItemMock)destination).IsDirty = anyChange;
        }

        public override Tuple<IWorkItemLink, IWorkItemLink> MakeLinks(IWorkItemLinkType workItemLinkType, IWorkItemExposed source, IWorkItemExposed destination)
        {
            return new Tuple<IWorkItemLink, IWorkItemLink>(
                new WorkItemLinkMock(workItemLinkType.ForwardEndImmutableName, destination.Id, this.Store),
                new WorkItemLinkMock(workItemLinkType.ReverseEndImmutableName, source.Id, this.Store));
        }

        public void RemoveWorkItemLink(IWorkItemExposed destination, string linkTypeName)
        {
            bool anyChange = this.DoRemoveWorkItemLink(destination, linkTypeName);
            this.IsDirty = anyChange;
            ((WorkItemMock)destination).IsDirty = anyChange;
        }

        public void RemoveWorkItemLinks(string linkTypeName)
        {
            throw new NotImplementedException();
        }

        public void AddHyperlink(string destination)
        {
            this.AddHyperlink(destination, string.Empty);
        }

        public void AddHyperlink(string destination, string comment)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return string.Format("WI #{0} [{1}]{2}", this.Id, this.TypeName, this.IsDirty ? "*" : string.Empty);
        }
    }
}
