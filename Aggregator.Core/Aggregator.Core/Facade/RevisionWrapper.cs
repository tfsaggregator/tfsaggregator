using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Aggregator.Core.Context;
using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class RevisionWrapper : IRevision
    {
        private readonly Revision revision;

        private readonly IRuntimeContext context;

        public RevisionWrapper(Revision revision, IRuntimeContext context)
        {
            this.revision = revision;
            this.context = context;
        }

        public object this[string name]
        {
            get
            {
                return this.revision[name];
            }
        }

        public IFieldCollection Fields
        {
            get
            {
                return new FieldCollectionWrapper(this.revision.Fields, this.context);
            }
        }

        public int Index
        {
            get
            {
                return this.revision.Index;
            }
        }

        public IWorkItemLinkExposedCollection WorkItemLinks
        {
            get
            {
                return new WorkItemLinkExposedCollectionWrapper(this.revision.Links, this.context);
            }
        }
    }
}