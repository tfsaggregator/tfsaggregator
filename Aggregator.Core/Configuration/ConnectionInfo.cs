using System;
using Microsoft.TeamFoundation.Framework.Client;

namespace Aggregator.Core.Configuration
{
    public class ConnectionInfo
    {
        public Uri ProjectCollectionUri { get; private set; }

        public IdentityDescriptor Impersonate { get; private set; }

        public ConnectionInfo(Uri uri, IdentityDescriptor toImpersonate)
        {
            this.ProjectCollectionUri = uri;
            this.Impersonate = toImpersonate;
        }
    }
}
