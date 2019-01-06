using System;
using System.Collections;
using System.Collections.Generic;

using Aggregator.Core;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;
using Aggregator.Core.Navigation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace UnitTests.Core.Mock
{
    internal class WorkItemMock : WorkItemImplementationBase, IWorkItem
    {
        private readonly FieldCollectionMock fields;

        public WorkItemMock(IWorkItemRepository repository, IRuntimeContext context)
            : base(context)
        {
            this.fields = new FieldCollectionMock(this);
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

        private readonly WorkItemLinkCollectionMock workItemLinks = new WorkItemLinkCollectionMock();

        public override IWorkItemLinkCollection WorkItemLinksImpl
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
            // HACK: should use the code in wrapper...
            var relationship = new WorkItemLinkMock(linkTypeName, destination.Id, this.Store);

            // check it does not exist already
            if (!this.workItemLinks.Contains(relationship))
            {
                this.workItemLinks.Add(relationship);
                this.IsDirty = true;
            }
        }

        public void AddHyperlink(string destination)
        {
            this.AddHyperlink(destination, string.Empty);
        }

        public void AddHyperlink(string destination, string comment)
        {
            throw new NotImplementedException();
        }

        public void RemoveWorkItemLink(IWorkItemLinkExposed link)
        {
            // HACK: should use the code in wrapper...
            var relationship = new WorkItemLinkMock(link.LinkTypeEndImmutableName, link.TargetId, this.Store);
            if (this.workItemLinks.Contains(relationship))
            {
                this.workItemLinks.Remove(relationship);
                this.IsDirty = true;
            }
        }

        public IWorkItemLinkExposedCollection WorkItemLinks
        {
            get { throw new NotImplementedException(); }
        }

    }
}
