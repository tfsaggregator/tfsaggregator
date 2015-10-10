using System;

using Aggregator.Core.Interfaces;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace Aggregator.Core.Facade
{
    public class RevisionWrapper : IRevision
    {
        private readonly Revision revision;

        public RevisionWrapper(Revision revision)
        {
            this.revision = revision;
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
                return new FieldCollectionWrapper(this.revision.Fields);
            }
        }

        public int Index
        {
            get
            {
                return this.revision.Index;
            }
        }
    }
}
